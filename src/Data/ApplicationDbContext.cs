using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PriMap.Data.Entities;

namespace PriMap.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : IdentityDbContext<ApplicationUser>(options)
    {
        public const int FeatureSrid = 3844;

        public DbSet<FeatureCategory> FeatureCategories => Set<FeatureCategory>();
        public DbSet<GisFeature> GisFeatures => Set<GisFeature>();
        public DbSet<FeatureAuditLog> FeatureAuditLogs => Set<FeatureAuditLog>();
        public DbSet<ImportBatch> ImportBatches => Set<ImportBatch>();
        public DbSet<ImportRowResult> ImportRowResults => Set<ImportRowResult>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            ConfigureIdentity(builder);
            ConfigureFeatureCategory(builder);
            ConfigureGisFeature(builder);
            ConfigureFeatureAuditLog(builder);
            ConfigureImportBatch(builder);
        }

        private static void ConfigureIdentity(ModelBuilder builder)
        {
            builder.Entity<ApplicationUser>(b =>
            {
                b.Property(u => u.DisplayName).IsRequired();
                b.HasIndex(u => u.IsActive);
            });
        }

        private static void ConfigureFeatureCategory(ModelBuilder builder)
        {
            builder.Entity<FeatureCategory>(b =>
            {
                b.HasIndex(c => c.Name).IsUnique();
                b.Property(c => c.Color).HasDefaultValue("#16897C");
            });
        }

        private static void ConfigureGisFeature(ModelBuilder builder)
        {
            builder.Entity<GisFeature>(b =>
            {
                b.Property(f => f.Geometry)
                    .HasColumnType("geometry");

                b.HasIndex(f => f.Status);
                b.HasIndex(f => f.CategoryId);

                b.HasOne(f => f.Category)
                    .WithMany(c => c.Features)
                    .HasForeignKey(f => f.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(f => f.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(f => f.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(f => f.ModifiedByUser)
                    .WithMany()
                    .HasForeignKey(f => f.ModifiedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(f => f.DeletedByUser)
                    .WithMany()
                    .HasForeignKey(f => f.DeletedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureFeatureAuditLog(ModelBuilder builder)
        {
            builder.Entity<FeatureAuditLog>(b =>
            {
                b.HasIndex(a => a.ChangeSetId);
                b.HasIndex(a => new { a.FeatureId, a.PerformedAt });

                b.HasOne(a => a.Feature)
                    .WithMany(f => f.AuditEntries)
                    .HasForeignKey(a => a.FeatureId)
                    .OnDelete(DeleteBehavior.SetNull);

                b.HasOne(a => a.PerformedByUser)
                    .WithMany()
                    .HasForeignKey(a => a.PerformedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureImportBatch(ModelBuilder builder)
        {
            builder.Entity<ImportBatch>(b =>
            {
                b.HasOne(i => i.TargetCategory)
                    .WithMany()
                    .HasForeignKey(i => i.TargetCategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(i => i.ImportedByUser)
                    .WithMany()
                    .HasForeignKey(i => i.ImportedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ImportRowResult>(b =>
            {
                b.HasIndex(r => new { r.ImportBatchId, r.RowNumber });

                b.HasOne(r => r.ImportBatch)
                    .WithMany(i => i.RowResults)
                    .HasForeignKey(r => r.ImportBatchId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
