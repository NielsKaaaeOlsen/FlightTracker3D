using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirCraftDetector
{
    public class NearestAirCraftDetectorResult   //TODO CTOR
    {
        public enum ResultType { NoAircraftsFound, newAircraftDetected, existingAirCraftDetected };

        public ResultType Type { get; set;  }
        public AircraftTrack? AircraftTrack { get; set; }
    }
}
