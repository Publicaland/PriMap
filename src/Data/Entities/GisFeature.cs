using System.ComponentModel.DataAnnotations;
using NetTopologySuite.Geometries;

namespace PriMap.Data.Entities
{
    public class GisFeature
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        public int CategoryId { get; set; }
        public FeatureCategory? Category { get; set; }

        public GeometryKind GeometryType { get; set; }

        [Required]
        public Geometry Geometry { get; set; } = null!;

        public FeatureStatus Status { get; set; } = FeatureStatus.Active;

        [Required, MaxLength(450)]
        public string CreatedByUserId { get; set; } = string.Empty;
        public ApplicationUser? CreatedByUser { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        [MaxLength(450)]
        public string? ModifiedByUserId { get; set; }
        public ApplicationUser? ModifiedByUser { get; set; }
        public DateTimeOffset? ModifiedAt { get; set; }

        [MaxLength(450)]
        public string? DeletedByUserId { get; set; }
        public ApplicationUser? DeletedByUser { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = [];

        public ICollection<FeatureAuditLog> AuditEntries { get; set; } = [];
    }
}
