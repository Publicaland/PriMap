using System.ComponentModel.DataAnnotations;

namespace PriMap.Data.Entities
{
    public class FeatureCategory
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(400)]
        public string? Description { get; set; }

        public GeometryKind AllowedGeometry { get; set; }

        [MaxLength(9)]
        public string Color { get; set; } = "#16897C";

        [MaxLength(100)]
        public string Icon { get; set; } = "Icons.Material.Filled.Place";

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; }

        public ICollection<GisFeature> Features { get; set; } = [];
    }
}
