using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirCraftDetector
{
    public class PositionPoint
    {
        public DateTime Timestamp { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        
        /// <summary>
        /// Altitude in feet (ADS-B standard)
        /// </summary>
        public int? Altitude { get; set; }
        
        /// <summary>
        /// Get altitude in meters for distance calculations
        /// </summary>
        public double AltitudeMeters => (Altitude ?? 0) * 0.3048;

        public override string ToString()
        {
            return $"Latitude: {Latitude:F4}°, Longitude: {Longitude:F4}°, Altitude: {(Altitude.HasValue ? $"{Altitude.Value:F1} feet " : "N/A")}";
        }

    }
}
