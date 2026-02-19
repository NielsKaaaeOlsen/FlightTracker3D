using AirCraftDetector;
using LCDController;
using LEDController;
using Microsoft.Extensions.Logging;
using StepperMotorController;
using HardwareMode;

namespace FlightTracker3D
{
    public class FlightTracking
    {
        private readonly NearestAirCraftDetector _nearestAirCraftDetector;
        private readonly TrackInfoPresenter _trackInfoPresenter;
        private readonly FlightTrackerLEDController _ledController;
        private readonly AzElController _azElController;


        public FlightTracking(HardwareModes hardwareModes, ILoggerFactory? loggerFactory)
        {

            //string host = "localhost";
            string host = "192.168.1.92";
            int port = 30003;

            PositionPoint referencePoint = new PositionPoint
            {
                Latitude = 55.860084097583304,
                Longitude = 12.4597123325542,
                Altitude = 50
            };

            _nearestAirCraftDetector = new NearestAirCraftDetector(host, port, referencePoint);
            _trackInfoPresenter = new TrackInfoPresenter(hardwareModes.LedHardwareMode);
            _ledController = new FlightTrackerLEDController(hardwareModes.LedHardwareMode);
            _azElController = new AzElController(loggerFactory, hardwareModes.StepperMotorHardwareMode);

        }
        public async Task StartTrackingAsync()
        {
            Console.WriteLine("Starting flight tracking...");

            while (true)
            {
            }
        }

    }
}
