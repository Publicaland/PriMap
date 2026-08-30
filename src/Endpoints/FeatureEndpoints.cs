using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using PriMap.Data;
using PriMap.Data.Spatial;
using System.Security.Claims;

namespace PriMap.Endpoints
{
    public static class FeatureEndpoints
    {
        public static void MapFeatureEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/features", GetFeaturesInViewportAsync)
                .RequireAuthorization(AppPolicies.CanView)
                .WithName("GetFeaturesGeoJson");

            app.MapGet("/api/categories", GetActiveCategoriesAsync)
                .RequireAuthorization(AppPolicies.CanView)
                .WithName("GetActiveCategories");
        }

        private static async Task<IResult> GetActiveCategoriesAsync(ApplicationDbContext db)
        {
            var categories = await db.FeatureCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    AllowedGeometry = c.AllowedGeometry.ToString(),
                    c.Color,
                    c.Icon,
                })
                .ToListAsync();

            return Results.Ok(categories);
        }

        private static async Task<IResult> GetFeaturesInViewportAsync(
            double minLon, double minLat, double maxLon, double maxLat,
            int? categoryId,
            ApplicationDbContext db,
            CoordinateReprojectionService reprojection,
            ClaimsPrincipal user)
        {
            var (x1, y1) = reprojection.Wgs84PointToStereo70(minLon, minLat);
            var (x2, y2) = reprojection.Wgs84PointToStereo70(maxLon, maxLat);
            var envelope = new Envelope(
                Math.Min(x1, x2), Math.Max(x1, x2),
                Math.Min(y1, y2), Math.Max(y1, y2));
            var envelopeGeometry = SpatialFactory.Instance.ToGeometry(envelope);

            var canSeeAudit = user.IsInRole(AppRoles.Editor)
                || user.IsInRole(AppRoles.Analyst)
                || user.IsInRole(AppRoles.Admin);

            var query = db.GisFeatures
                .Include(f => f.Category)
                .Include(f => f.CreatedByUser)
                .Include(f => f.ModifiedByUser)
                .Where(f => f.Status == FeatureStatus.Active)
                .Where(f => f.Geometry.Intersects(envelopeGeometry));

            if (categoryId is not null)
            {
                query = query.Where(f => f.CategoryId == categoryId);
            }

            const int maxFeatures = 2000;

            var rows = await query
                .OrderBy(f => f.Id)
                .Take(maxFeatures)
                .ToListAsync();

            var collection = new FeatureCollection();
            foreach (var row in rows)
            {
                var attributes = new AttributesTable
                {
                    { "id", row.Id },
                    { "name", row.Name },
                    { "description", row.Description ?? "" },
                    { "categoryId", row.CategoryId },
                    { "categoryName", row.Category?.Name ?? "" },
                    { "color", row.Category?.Color ?? "#16897C" },
                    { "geometryType", row.GeometryType.ToString() },
                };

                if (canSeeAudit)
                {
                    attributes.Add("createdBy", row.CreatedByUser?.DisplayName ?? "");
                    attributes.Add("createdAt", row.CreatedAt.ToString("O"));
                    attributes.Add("modifiedBy", row.ModifiedByUser?.DisplayName ?? "");
                    attributes.Add("modifiedAt", row.ModifiedAt?.ToString("O") ?? "");
                }

                var wgs84Geometry = reprojection.ToWgs84(row.Geometry);
                collection.Add(new Feature(wgs84Geometry, attributes));
            }

            return Results.Json(collection);
        }
    }
}
