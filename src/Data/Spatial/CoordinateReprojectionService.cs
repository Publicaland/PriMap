using NetTopologySuite.Geometries;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

namespace PriMap.Data.Spatial
{
    public sealed class CoordinateReprojectionService
    {
        public const int Wgs84Srid = 4326;

        private readonly MathTransform _toWgs84;
        private readonly MathTransform _toStereo70;

        public CoordinateReprojectionService()
        {
            var csFactory = new CoordinateSystemFactory();
            var stereo70 = csFactory.CreateFromWkt(Stereo70Wkt);
            var wgs84 = GeographicCoordinateSystem.WGS84;

            var ctFactory = new CoordinateTransformationFactory();
            _toWgs84 = ctFactory.CreateFromCoordinateSystems(stereo70, wgs84).MathTransform;
            _toStereo70 = ctFactory.CreateFromCoordinateSystems(wgs84, stereo70).MathTransform;
        }

        public Geometry ToWgs84(Geometry stereo70Geometry) =>
            Reproject(stereo70Geometry, _toWgs84, Wgs84Srid);

        public Geometry ToStereo70(Geometry wgs84Geometry) =>
            Reproject(wgs84Geometry, _toStereo70, ApplicationDbContext.FeatureSrid);

        public (double X, double Y) Wgs84PointToStereo70(double lon, double lat)
        {
            var result = _toStereo70.Transform([lon, lat]);
            return (result[0], result[1]);
        }

        private static Geometry Reproject(Geometry source, MathTransform transform, int targetSrid)
        {
            var targetFactory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(targetSrid);

            Coordinate TransformCoordinate(Coordinate c)
            {
                var result = transform.Transform([c.X, c.Y]);
                return new Coordinate(result[0], result[1]);
            }

            switch (source)
            {
                case Point p:
                    return targetFactory.CreatePoint(TransformCoordinate(p.Coordinate));

                case LineString ls:
                    return targetFactory.CreateLineString(
                        Array.ConvertAll(ls.Coordinates, TransformCoordinate));

                case Polygon poly:
                    var shell = targetFactory.CreateLinearRing(
                        Array.ConvertAll(poly.ExteriorRing.Coordinates, TransformCoordinate));
                    var holes = new LinearRing[poly.NumInteriorRings];
                    for (var i = 0; i < poly.NumInteriorRings; i++)
                    {
                        holes[i] = targetFactory.CreateLinearRing(
                            Array.ConvertAll(poly.GetInteriorRingN(i).Coordinates, TransformCoordinate));
                    }
                    return targetFactory.CreatePolygon(shell, holes);

                case MultiPoint mp:
                    return targetFactory.CreateMultiPointFromCoords(
                        Array.ConvertAll(mp.Coordinates, TransformCoordinate));

                case MultiLineString mls:
                    return targetFactory.CreateMultiLineString(Array.ConvertAll(
                        mls.Geometries, g => (LineString)Reproject(g, transform, targetSrid)));

                case MultiPolygon mpoly:
                    return targetFactory.CreateMultiPolygon(Array.ConvertAll(
                        mpoly.Geometries, g => (Polygon)Reproject(g, transform, targetSrid)));

                default:
                    throw new NotSupportedException(
                        $"Geometria de tip '{source.GeometryType}' nu este suportată pentru reproiecție.");
            }
        }

        private const string Stereo70Wkt = """
            PROJCS["Pulkovo 1942(58) / Stereo70",
                GEOGCS["Pulkovo 1942(58)",
                    DATUM["Pulkovo_1942_58",
                        SPHEROID["Krassowsky 1940",6378245,298.3],
                        TOWGS84[2.329,-147.042,-92.08,0.309,-0.325,-0.497,5.69]],
                    PRIMEM["Greenwich",0,
                        AUTHORITY["EPSG","8901"]],
                    UNIT["degree",0.0174532925199433,
                        AUTHORITY["EPSG","9122"]],
                    AUTHORITY["EPSG","4179"]],
                PROJECTION["Oblique_Stereographic"],
                PARAMETER["latitude_of_origin",46],
                PARAMETER["central_meridian",25],
                PARAMETER["scale_factor",0.99975],
                PARAMETER["false_easting",500000],
                PARAMETER["false_northing",500000],
                UNIT["metre",1,
                    AUTHORITY["EPSG","9001"]],
                AUTHORITY["EPSG","3844"]]
            """;
    }
}
