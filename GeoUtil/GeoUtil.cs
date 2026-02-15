using System;

namespace GeoUtil
{
    /*
     * WGS84 Geodetic to ECEF Cartesian Conversion
     * ============================================
     * 
     * Formulas:
     *   N = a / sqrt(1 - e² sin²φ)
     *   X = (N + h) cos φ cos λ
     *   Y = (N + h) cos φ sin λ
     *   Z = (N(1 - e²) + h) sin φ
     * 
     * Where:
     *   a = semi-major axis (6378137.0 m for WGS84)
     *   e² = eccentricity squared (0.00669437999014)
     *   φ = latitude (radians)
     *   λ = longitude (radians)
     *   h = altitude above ellipsoid (meters)
     *   N = radius of curvature in prime vertical
     * 
     * Azimuth and Elevation:
     * ----------------------
     * Azimuth: Horizontal bearing from point 1 to point 2, measured clockwise from true north (0–360°).
     * Elevation: Vertical angle from horizontal at point 1 to point 2 (-90° to +90°, positive = above horizon).
     * 
     * References:
     * -----------
     * [1] NIMA TR8350.2 — "Department of Defense World Geodetic System 1984"
     *     https://earth-info.nga.mil/php/download.php?file=coord-wgs84
     * 
     * [2] Geographic coordinate conversion (Wikipedia)
     *     https://en.wikipedia.org/wiki/Geographic_coordinate_conversion#From_geodetic_to_ECEF_coordinates
     * 
     * [3] World Geodetic System (Wikipedia)
     *     https://en.wikipedia.org/wiki/World_Geodetic_System
     * 
     * [4] Hofmann-Wellenhof, B., et al. (2008). "GNSS – Global Navigation Satellite Systems"
     *     Springer. Chapters 2–3.
     * 
     * [5] Jekeli, C. (2012). "Geometric Reference Systems in Geodesy"
     *     http://www.geology.osu.edu/~jekeli.1/OSUReports/reports/report_478.pdf
     * 
     * [6] Haversine formula (great-circle distance)
     *     https://en.wikipedia.org/wiki/Haversine_formula
     * 
     * [7] Azimuth calculation (forward azimuth)
     *     https://en.wikipedia.org/wiki/Azimuth#Calculation
     * 
     * Online verification tools:
     *   - NOAA: https://www.ngs.noaa.gov/NCAT/
     *   - NPS:  https://www.oc.nps.edu/oc2902w/coord/llhxyz.htm
     */

    /// <summary>
    /// Calculates straight-line (Euclidean) distance between two WGS84 points.
    /// </summary>
    public class GeoDistance
    {
        // WGS84 ellipsoid parameters (from NIMA TR8350.2)
        private const double SemiMajorAxis = 6378137.0;           // meters (a)
        private const double SemiMinorAxis = 6356752.314245;      // meters (b)
        private const double Flattening = 1.0 / 298.257223563;    // f = (a - b) / a
        private const double EccentricitySquared = 6.69437999014e-3; // e² = (a² - b²) / a²

        /// <summary>
        /// Calculate straight-line distance in meters between two WGS84 points.
        /// Uses ECEF (Earth-Centered, Earth-Fixed) Cartesian coordinates for 3D Euclidean distance.
        /// </summary>
        /// <param name="lat1">Latitude of point 1 (degrees)</param>
        /// <param name="lon1">Longitude of point 1 (degrees)</param>
        /// <param name="alt1">Altitude of point 1 above WGS84 ellipsoid (meters)</param>
        /// <param name="lat2">Latitude of point 2 (degrees)</param>
        /// <param name="lon2">Longitude of point 2 (degrees)</param>
        /// <param name="alt2">Altitude of point 2 above WGS84 ellipsoid (meters)</param>
        /// <returns>Distance in meters</returns>
        public static double CalculateDistance(
            double lat1, double lon1, double alt1,
            double lat2, double lon2, double alt2)
        {
            var p1 = GeodeticToEcef(lat1, lon1, alt1);
            var p2 = GeodeticToEcef(lat2, lon2, alt2);

            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            double dz = p2.Z - p1.Z;

            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        /// <summary>
        /// Calculate azimuth (bearing) from point 1 to point 2.
        /// Azimuth is measured clockwise from true north (0° = north, 90° = east, 180° = south, 270° = west).
        /// </summary>
        /// <param name="lat1">Latitude of point 1 (degrees)</param>
        /// <param name="lon1">Longitude of point 1 (degrees)</param>
        /// <param name="lat2">Latitude of point 2 (degrees)</param>
        /// <param name="lon2">Longitude of point 2 (degrees)</param>
        /// <returns>Azimuth in degrees (0–360)</returns>
        public static double CalculateAzimuth(double lat1, double lon1, double lat2, double lon2)
        {
            double lat1Rad = lat1 * Math.PI / 180.0;
            double lat2Rad = lat2 * Math.PI / 180.0;
            double deltaLon = (lon2 - lon1) * Math.PI / 180.0;

            double y = Math.Sin(deltaLon) * Math.Cos(lat2Rad);
            double x = Math.Cos(lat1Rad) * Math.Sin(lat2Rad) -
                       Math.Sin(lat1Rad) * Math.Cos(lat2Rad) * Math.Cos(deltaLon);

            double azimuthRad = Math.Atan2(y, x);
            double azimuthDeg = azimuthRad * 180.0 / Math.PI;

            // Normalize to 0–360
            return (azimuthDeg + 360.0) % 360.0;
        }

        /// <summary>
        /// Calculate elevation angle from point 1 to point 2.
        /// Positive = above horizon, negative = below horizon.
        /// </summary>
        /// <param name="lat1">Latitude of point 1 (degrees)</param>
        /// <param name="lon1">Longitude of point 1 (degrees)</param>
        /// <param name="alt1">Altitude of point 1 above WGS84 ellipsoid (meters)</param>
        /// <param name="lat2">Latitude of point 2 (degrees)</param>
        /// <param name="lon2">Longitude of point 2 (degrees)</param>
        /// <param name="alt2">Altitude of point 2 above WGS84 ellipsoid (meters)</param>
        /// <returns>Elevation angle in degrees (-90 to +90)</returns>
        public static double CalculateElevation(
            double lat1, double lon1, double alt1,
            double lat2, double lon2, double alt2)
        {
            // Horizontal distance using Haversine (approximation; for high accuracy use geodesic)
            double horizontalDist = CalculateHaversineDistance(lat1, lon1, lat2, lon2);
            double altitudeDiff = alt2 - alt1;

            double elevationRad = Math.Atan2(altitudeDiff, horizontalDist);
            return elevationRad * 180.0 / Math.PI;
        }

        /// <summary>
        /// Calculate both azimuth and elevation from point 1 to point 2.
        /// </summary>
        /// <param name="lat1">Latitude of point 1 (degrees)</param>
        /// <param name="lon1">Longitude of point 1 (degrees)</param>
        /// <param name="alt1">Altitude of point 1 above WGS84 ellipsoid (meters)</param>
        /// <param name="lat2">Latitude of point 2 (degrees)</param>
        /// <param name="lon2">Longitude of point 2 (degrees)</param>
        /// <param name="alt2">Altitude of point 2 above WGS84 ellipsoid (meters)</param>
        /// <returns>Tuple: (Azimuth in degrees 0-360, Elevation in degrees -90 to +90)</returns>
        public static (double Azimuth, double Elevation, double Distance) CalculateAzimuthAndElevation(
            double lat1, double lon1, double alt1,
            double lat2, double lon2, double alt2)
        {
            double azimuth = CalculateAzimuth(lat1, lon1, lat2, lon2);
            double elevation = CalculateElevation(lat1, lon1, alt1, lat2, lon2, alt2);
            double distance = CalculateDistance(lat1, lon1, alt1, lat2, lon2, alt2);
            return (azimuth, elevation, distance);
        }

        /// <summary>
        /// Convert WGS84 geodetic coordinates to ECEF Cartesian coordinates.
        /// Based on formulas from NIMA TR8350.2 Section 4.1.
        /// </summary>
        /// <param name="latDeg">Latitude in degrees</param>
        /// <param name="lonDeg">Longitude in degrees</param>
        /// <param name="altMeters">Altitude above WGS84 ellipsoid in meters</param>
        /// <returns>ECEF coordinates (X, Y, Z) in meters</returns>
        public static (double X, double Y, double Z) GeodeticToEcef(double latDeg, double lonDeg, double altMeters)
        {
            double latRad = latDeg * Math.PI / 180.0;
            double lonRad = lonDeg * Math.PI / 180.0;

            double sinLat = Math.Sin(latRad);
            double cosLat = Math.Cos(latRad);
            double sinLon = Math.Sin(lonRad);
            double cosLon = Math.Cos(lonRad);

            // Radius of curvature in prime vertical (N)
            double N = SemiMajorAxis / Math.Sqrt(1.0 - EccentricitySquared * sinLat * sinLat);

            double x = (N + altMeters) * cosLat * cosLon;
            double y = (N + altMeters) * cosLat * sinLon;
            double z = (N * (1.0 - EccentricitySquared) + altMeters) * sinLat;

            return (x, y, z);
        }

        /// <summary>
        /// Calculates horizontal (great-circle) distance using Haversine formula (ignores altitude).
        /// Returns the shortest distance over the Earth's surface (approximated as a sphere).
        /// See: https://en.wikipedia.org/wiki/Haversine_formula
        /// </summary>
        /// <param name="lat1">Latitude of point 1 (degrees)</param>
        /// <param name="lon1">Longitude of point 1 (degrees)</param>
        /// <param name="lat2">Latitude of point 2 (degrees)</param>
        /// <param name="lon2">Longitude of point 2 (degrees)</param>
        /// <returns>Distance in meters</returns>
        public static double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            double lat1Rad = lat1 * Math.PI / 180.0;
            double lat2Rad = lat2 * Math.PI / 180.0;
            double deltaLat = (lat2 - lat1) * Math.PI / 180.0;
            double deltaLon = (lon2 - lon1) * Math.PI / 180.0;

            double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                       Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                       Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return SemiMajorAxis * c;
        }
    }
}