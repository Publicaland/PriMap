using System.ComponentModel.DataAnnotations;

namespace PriMap.Data.Entities
{
    public class FeatureAuditLog
    {
        public long Id { get; set; }

        public Guid ChangeSetId { get; set; }

        public int? FeatureId { get; set; }
        public GisFeature? Feature { get; set; }

        [MaxLength(200)]
        public string FeatureLabel { get; set; } = string.Empty;

        public AuditAction Action { get; set; }

        [MaxLength(100)]
        public string? FieldName { get; set; }

        public string? OldValue { get; set; }

        public string? NewValue { get; set; }

        [Required, MaxLength(450)]
        public string PerformedByUserId { get; set; } = string.Empty;
        public ApplicationUser? PerformedByUser { get; set; }

        public DateTimeOffset PerformedAt { get; set; } = DateTimeOffset.UtcNow;

        [MaxLength(45)]
        public string? ClientIpAddress { get; set; }
    }
}
