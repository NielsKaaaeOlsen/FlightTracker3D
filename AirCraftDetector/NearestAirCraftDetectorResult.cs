using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirCraftDetector
{
    public class NearestAirCraftDetectorResult
    {
        public NearestAirCraftDetectorResult(AircraftTrack aircraftTrack, AzElPosition aircraftAzElPosition)
        {
            this.AircraftTrack = aircraftTrack;
            this.AircraftAzElPosition = aircraftAzElPosition; 
        }
        public AircraftTrack AircraftTrack { get; }

        public AzElPosition AircraftAzElPosition { get; }
    }
}
