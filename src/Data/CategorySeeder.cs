using Microsoft.EntityFrameworkCore;
using PriMap.Data.Entities;

namespace PriMap.Data
{
    public static class CategorySeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var db = services.GetRequiredService<ApplicationDbContext>();

            if (await db.FeatureCategories.AnyAsync())
            {
                return;
            }

            db.FeatureCategories.AddRange(
                new FeatureCategory
                {
                    Name = "Stâlp de iluminat",
                    Description = "Puncte de iluminat public",
                    AllowedGeometry = GeometryKind.Point,
                    Color = "#F2A93B",
                    Icon = "Icons.Material.Filled.LightMode",
                    DisplayOrder = 1,
                },
                new FeatureCategory
                {
                    Name = "Segment stradal",
                    Description = "Tronsoane de stradă / drum",
                    AllowedGeometry = GeometryKind.Line,
                    Color = "#0277BD",
                    Icon = "Icons.Material.Filled.Route",
                    DisplayOrder = 2,
                },
                new FeatureCategory
                {
                    Name = "Parcelă cadastrală",
                    Description = "Limite de parcelă",
                    AllowedGeometry = GeometryKind.Polygon,
                    Color = "#16897C",
                    Icon = "Icons.Material.Filled.Fence",
                    DisplayOrder = 3,
                });

            await db.SaveChangesAsync();
        }
    }
}
