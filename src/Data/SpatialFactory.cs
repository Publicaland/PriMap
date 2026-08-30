using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace PriMap.Data
{
    public static class SpatialFactory
    {
        public const int Srid = ApplicationDbContext.FeatureSrid;

        public static readonly GeometryFactory Instance =
            NtsGeometryServices.Instance.CreateGeometryFactory(Srid);
    }
}
