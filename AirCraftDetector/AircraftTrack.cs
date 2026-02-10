using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirCraftDetector
{
    public class AircraftTrack
    {
        public string Icao { get; set; }
        public List<PositionPoint> History { get; } = new();
        public DateTime LastSeen { get; set; }
    }

}
