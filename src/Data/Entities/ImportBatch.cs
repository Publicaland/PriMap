using System.ComponentModel.DataAnnotations;

namespace PriMap.Data.Entities
{
    public class ImportBatch
    {
        public int Id { get; set; }

        [Required, MaxLength(260)]
        public string FileName { get; set; } = string.Empty;

        public int TargetCategoryId { get; set; }
        public FeatureCategory? TargetCategory { get; set; }

        [Required, MaxLength(450)]
        public string ImportedByUserId { get; set; } = string.Empty;
        public ApplicationUser? ImportedByUser { get; set; }

        public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? CompletedAt { get; set; }

        public int TotalRows { get; set; }
        public int ImportedRows { get; set; }
        public int SkippedRows { get; set; }
        public int FailedRows { get; set; }

        public int SourceSrid { get; set; } = 3844;

        public ICollection<ImportRowResult> RowResults { get; set; } = [];
    }

    public class ImportRowResult
    {
        public long Id { get; set; }

        public int ImportBatchId { get; set; }
        public ImportBatch? ImportBatch { get; set; }

        public int RowNumber { get; set; }

        public ImportRowStatus Status { get; set; }

        public int? CreatedFeatureId { get; set; }

        [MaxLength(1000)]
        public string? Message { get; set; }
    }
}
