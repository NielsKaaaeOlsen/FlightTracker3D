using FlightTracker3D;
using HardwareMode;
using Microsoft.Extensions.Logging;

namespace FlightTracker3DApp
{
    public class FlightTracker3DAppSettings
    {
        public required LogLevel MinimumLogLevel { get; set; }
        public required HardwareModes HardwareModes { get; set; }
    }
}
