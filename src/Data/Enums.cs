namespace PriMap.Data
{
    public enum GeometryKind
    {
        Point = 0,
        Line = 1,
        Polygon = 2
    }

    public enum FeatureStatus
    {
        Active = 0,
        Deleted = 1
    }

    public enum AuditAction
    {
        Create = 0,
        Update = 1,
        SoftDelete = 2,
        Restore = 3,
        HardDelete = 4
    }

    public enum ImportRowStatus
    {
        Imported = 0,
        Skipped = 1,
        Failed = 2
    }

    public static class AppRoles
    {
        public const string Viewer = "Viewer";
        public const string Editor = "Editor";
        public const string Analyst = "Analyst";
        public const string Admin = "Admin";

        public static readonly string[] All = [Viewer, Editor, Analyst, Admin];
    }

    public static class AppPolicies
    {
        public const string CanView = "CanView";
        public const string CanEdit = "CanEdit";
        public const string CanAnalyze = "CanAnalyze";
        public const string CanManage = "CanManage";
    }
}
