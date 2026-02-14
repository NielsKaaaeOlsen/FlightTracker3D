using GeoUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirCraftDetector
{
    public static class PositionPointExtensions
    {
        /// <summary>
        /// Calculate straight-line distance in meters between two PositionPoint objects.
        /// </summary>
        public static double DistanceTo(this PositionPoint from, PositionPoint to)
        {
            double alt1 = from.Altitude ?? 0.0;
            double alt2 = to.Altitude ?? 0.0;

            return GeoDistance.CalculateDistance(
                from.Latitude, from.Longitude, alt1,
                to.Latitude, to.Longitude, alt2
            );
        }

        /// <summary>
        /// Calculate azimuth and elevation from this point to target point.
        /// </summary>
        public static (double Azimuth, double Elevation) AzimuthAndElevationTo(
            this PositionPoint from, PositionPoint to)
        {
            double alt1 = from.Altitude ?? 0.0;
            double alt2 = to.Altitude ?? 0.0;

            return GeoDistance.CalculateAzimuthAndElevation(
                from.Latitude, from.Longitude, alt1,
                to.Latitude, to.Longitude, alt2
            );
        }
    }
}
