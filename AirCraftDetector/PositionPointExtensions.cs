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
            return GeoDistance.CalculateDistance(
                from.Latitude, from.Longitude, from.AltitudeMeters,
                to.Latitude, to.Longitude, to.AltitudeMeters
            );
        }

        /// <summary>
        /// Calculate azimuth and elevation from this point to target point.
        /// </summary>
        public static (double Azimuth, double Elevation, double Distance) AzimuthAndElevationTo(
            this PositionPoint from, PositionPoint to)
        {
            return GeoDistance.CalculateAzimuthAndElevation(
                from.Latitude, from.Longitude, from.AltitudeMeters,
                to.Latitude, to.Longitude, to.AltitudeMeters
            );
        }

        public static AzElPosition ToAzElPosition(this PositionPoint from, PositionPoint to)
        {
            var (azimuth, elevation, distance) = from.AzimuthAndElevationTo(to);
            return new AzElPosition
            {
                Timestamp = to.Timestamp,
                Azimuth = azimuth,
                Elevation = elevation,
                Distance = distance
            };
        }
    }
}
