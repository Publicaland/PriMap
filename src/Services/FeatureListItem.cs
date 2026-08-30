using PriMap.Data;

namespace PriMap.Services
{
    public record FeatureListItem(
        int Id,
        string Name,
        string? Description,
        int CategoryId,
        string CategoryName,
        string CategoryColor,
        GeometryKind GeometryType,
        FeatureStatus Status,
        string CreatedByDisplayName,
        DateTimeOffset CreatedAt,
        string? ModifiedByDisplayName,
        DateTimeOffset? ModifiedAt);

    public enum FeatureListSortField
    {
        Name,
        CategoryName,
        CreatedAt,
        ModifiedAt
    }

    public enum FeatureListStatusFilter
    {
        ActiveOnly,
        DeletedOnly,
        All
    }

    public record FeaturePageRequest(
        int Page,
        int PageSize,
        string? SearchText,
        int? CategoryId,
        FeatureListStatusFilter StatusFilter,
        FeatureListSortField SortField,
        bool SortDescending);

    public record FeaturePageResult(List<FeatureListItem> Items, int TotalCount);
}
