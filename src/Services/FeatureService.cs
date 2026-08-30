using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using PriMap.Data;
using PriMap.Data.Entities;
using PriMap.Data.Spatial;

namespace PriMap.Services
{
    public class FeatureService(
        IDbContextFactory<ApplicationDbContext> dbFactory,
        CoordinateReprojectionService reprojection,
        IHttpContextAccessor httpContextAccessor)
    {
        public async Task<List<FeatureCategory>> GetActiveCategoriesAsync()
        {
            await using var db = await dbFactory.CreateDbContextAsync();

            return await db.FeatureCategories
                .AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
        }

        public async Task<FeaturePageResult> GetPagedAsync(FeaturePageRequest request, CancellationToken cancellationToken = default)
        {
            await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
            var query = db.GisFeatures.AsNoTracking().Include(f => f.Category).AsQueryable();

            query = request.StatusFilter switch
            {
                FeatureListStatusFilter.ActiveOnly => query.Where(f => f.Status == FeatureStatus.Active),
                FeatureListStatusFilter.DeletedOnly => query.Where(f => f.Status == FeatureStatus.Deleted),
                _ => query,
            };

            if (request.CategoryId is { } categoryId)
            {
                query = query.Where(f => f.CategoryId == categoryId);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(f => EF.Functions.Like(f.Name, $"%{term}%")
                    || (f.Description != null && EF.Functions.Like(f.Description, $"%{term}%")));
            }

            query = (request.SortField, request.SortDescending) switch
            {
                (FeatureListSortField.Name, false) => query.OrderBy(f => f.Name),
                (FeatureListSortField.Name, true) => query.OrderByDescending(f => f.Name),
                (FeatureListSortField.CategoryName, false) => query.OrderBy(f => f.Category!.Name),
                (FeatureListSortField.CategoryName, true) => query.OrderByDescending(f => f.Category!.Name),
                (FeatureListSortField.ModifiedAt, false) => query.OrderBy(f => f.ModifiedAt),
                (FeatureListSortField.ModifiedAt, true) => query.OrderByDescending(f => f.ModifiedAt),
                (_, false) => query.OrderBy(f => f.CreatedAt),
                (_, true) => query.OrderByDescending(f => f.CreatedAt),
            };

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip(request.Page * request.PageSize)
                .Take(request.PageSize)
                .Select(f => new FeatureListItem(
                    f.Id,
                    f.Name,
                    f.Description,
                    f.CategoryId,
                    f.Category!.Name,
                    f.Category!.Color,
                    f.GeometryType,
                    f.Status,
                    f.CreatedByUser!.DisplayName,
                    f.CreatedAt,
                    f.ModifiedByUser != null ? f.ModifiedByUser.DisplayName : null,
                    f.ModifiedAt))
                //.ToListAsync();
                .ToListAsync(cancellationToken);

            return new FeaturePageResult(items, totalCount);
        }

        public async Task<GisFeature?> GetByIdAsync(int id)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            return await db.GisFeatures
                .AsNoTracking()
                .Include(f => f.Category)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<GisFeature> CreateAsync(int categoryId, string name, string? description, Geometry wgs84Geometry, string userId)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var category = await db.FeatureCategories.FindAsync(categoryId)
                ?? throw new InvalidOperationException("Categoria selectată nu există.");

            var geometryKind = GeometryKindOf(wgs84Geometry);
            if (geometryKind != category.AllowedGeometry)
            {
                throw new InvalidOperationException(
                    $"Categoria „{category.Name}” acceptă doar geometrii de tip {category.AllowedGeometry}.");
            }

            var now = DateTimeOffset.UtcNow;
            var feature = new GisFeature
            {
                Name = name,
                Description = description,
                CategoryId = categoryId,
                GeometryType = geometryKind,
                Geometry = reprojection.ToStereo70(wgs84Geometry),
                CreatedByUserId = userId,
                CreatedAt = now,
            };

            db.GisFeatures.Add(feature);
            db.FeatureAuditLogs.Add(new FeatureAuditLog
            {
                ChangeSetId = Guid.NewGuid(),
                Feature = feature,
                FeatureLabel = name,
                Action = AuditAction.Create,
                NewValue = name,
                PerformedByUserId = userId,
                PerformedAt = now,
                ClientIpAddress = ClientIp(),
            });

            await db.SaveChangesAsync();
            return feature;
        }

        public async Task UpdateAsync(int id, string name, string? description, int categoryId, Geometry? wgs84Geometry, byte[] rowVersion, string userId)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var feature = await db.GisFeatures.FirstOrDefaultAsync(f => f.Id == id)
                ?? throw new InvalidOperationException("Feature-ul nu a fost găsit.");

            db.Entry(feature).Property(f => f.RowVersion).OriginalValue = rowVersion;

            var changeSetId = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;
            var ip = ClientIp();

            void LogFieldChange(string field, string? oldValue, string? newValue)
            {
                if (oldValue == newValue)
                {
                    return;
                }

                db.FeatureAuditLogs.Add(new FeatureAuditLog
                {
                    ChangeSetId = changeSetId,
                    FeatureId = id,
                    FeatureLabel = name,
                    Action = AuditAction.Update,
                    FieldName = field,
                    OldValue = oldValue,
                    NewValue = newValue,
                    PerformedByUserId = userId,
                    PerformedAt = now,
                    ClientIpAddress = ip,
                });
            }

            LogFieldChange(nameof(GisFeature.Name), feature.Name, name);
            LogFieldChange(nameof(GisFeature.Description), feature.Description, description);
            LogFieldChange(nameof(GisFeature.CategoryId), feature.CategoryId.ToString(), categoryId.ToString());

            feature.Name = name;
            feature.Description = description;
            feature.CategoryId = categoryId;

            if (wgs84Geometry is not null)
            {
                var newGeometry = reprojection.ToStereo70(wgs84Geometry);
                LogFieldChange(nameof(GisFeature.Geometry), feature.Geometry.AsText(), newGeometry.AsText());
                feature.Geometry = newGeometry;
            }

            feature.ModifiedByUserId = userId;
            feature.ModifiedAt = now;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new InvalidOperationException(
                    "Acest element a fost modificat de altcineva între timp. Reîncărcați harta și încercați din nou.", ex);
            }
        }

        public async Task SoftDeleteAsync(int id, string userId)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var feature = await db.GisFeatures.FindAsync(id)
                ?? throw new InvalidOperationException("Feature-ul nu a fost găsit.");

            var now = DateTimeOffset.UtcNow;
            feature.Status = FeatureStatus.Deleted;
            feature.DeletedByUserId = userId;
            feature.DeletedAt = now;

            db.FeatureAuditLogs.Add(new FeatureAuditLog
            {
                ChangeSetId = Guid.NewGuid(),
                FeatureId = id,
                FeatureLabel = feature.Name,
                Action = AuditAction.SoftDelete,
                PerformedByUserId = userId,
                PerformedAt = now,
                ClientIpAddress = ClientIp(),
            });

            await db.SaveChangesAsync();
        }

        public async Task RestoreAsync(int id, string userId)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var feature = await db.GisFeatures.FindAsync(id)
                ?? throw new InvalidOperationException("Feature-ul nu a fost găsit.");

            var now = DateTimeOffset.UtcNow;
            feature.Status = FeatureStatus.Active;
            feature.DeletedByUserId = null;
            feature.DeletedAt = null;

            db.FeatureAuditLogs.Add(new FeatureAuditLog
            {
                ChangeSetId = Guid.NewGuid(),
                FeatureId = id,
                FeatureLabel = feature.Name,
                Action = AuditAction.Restore,
                PerformedByUserId = userId,
                PerformedAt = now,
                ClientIpAddress = ClientIp(),
            });

            await db.SaveChangesAsync();
        }

        public async Task HardDeleteAsync(int id, string userId)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var feature = await db.GisFeatures.FindAsync(id)
                ?? throw new InvalidOperationException("Feature-ul nu a fost găsit.");

            db.FeatureAuditLogs.Add(new FeatureAuditLog
            {
                ChangeSetId = Guid.NewGuid(),
                FeatureId = id,
                FeatureLabel = feature.Name,
                Action = AuditAction.HardDelete,
                PerformedByUserId = userId,
                PerformedAt = DateTimeOffset.UtcNow,
                ClientIpAddress = ClientIp(),
            });
            await db.SaveChangesAsync();

            db.GisFeatures.Remove(feature);
            await db.SaveChangesAsync();
        }

        private static GeometryKind GeometryKindOf(Geometry geometry) => geometry switch
        {
            Point => GeometryKind.Point,
            LineString => GeometryKind.Line,
            Polygon => GeometryKind.Polygon,
            _ => throw new InvalidOperationException($"Tip de geometrie neacceptat: {geometry.GeometryType}"),
        };

        private string? ClientIp() => httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
    }
}
