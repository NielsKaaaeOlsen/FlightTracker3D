using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirCraftDetector
{
    public class NearestAirCraftDetectorResult
    {
        public NearestAirCraftDetectorResult(AircraftTrack aircraftTrack, PositionPoint aircraftForecastPosition, AzElPosition aircraftAzElForecastPosition)
        {
            this.AircraftTrack = aircraftTrack;
            this.AircraftForecastPosition = aircraftForecastPosition;
            this.AircraftAzElForecastPosition = aircraftAzElForecastPosition; 
        }
        public AircraftTrack AircraftTrack { get; }

        public AzElPosition AircraftAzElForecastPosition { get; }

        public PositionPoint AircraftForecastPosition { get; }
    }
}
