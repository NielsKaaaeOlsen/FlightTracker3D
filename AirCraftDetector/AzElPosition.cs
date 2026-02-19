using System;

namespace AirCraftDetector
{
    /// <summary>
    /// Represents a position in horizontal coordinates (azimuth and elevation).
    /// Azimuth: Horizontal bearing from north (0-360°)
    /// Elevation: Vertical angle from horizon (-90° to +90°)
    /// </summary>
    public class AzElPosition
    {
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Azimuth in degrees (0-360°). 0° = North, 90° = East, 180° = South, 270° = West
        /// </summary>
        public double Azimuth { get; set; }
        
        /// <summary>
        /// Elevation in degrees (-90° to +90°). Positive = above horizon, negative = below
        /// </summary>
        public double Elevation { get; set; }
        
        /// <summary>
        /// Optional distance from observer in meters
        /// </summary>
        public double Distance { get; set; }


        public override string ToString()
        {
            //return $"Azimuth: {Azimuth:F1}°, Elevation: {Elevation:F1}°, Distance: {(Distance.HasValue ? $"{Distance.Value:F1} m" : "N/A")}";
            return $"Azimuth: {Azimuth:F1}°, Elevation: {Elevation:F1}°, Distance: {Distance:F1} m";
        }
    }
}