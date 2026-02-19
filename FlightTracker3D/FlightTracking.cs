using AirCraftDetector;
using LCDController;
using LEDController;
using Microsoft.Extensions.Logging;
using StepperMotorController;
using HardwareMode;

namespace FlightTracker3D
{
    public class FlightTracking : IDisposable
    {
        private readonly NearestAirCraftDetector _nearestAirCraftDetector;
        private readonly TrackInfoPresenter _lcdController;
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
            _lcdController = new TrackInfoPresenter(hardwareModes.LcdHardwareMode);
            _ledController = new FlightTrackerLEDController(hardwareModes.LedHardwareMode);
            _azElController = new AzElController(loggerFactory, hardwareModes.StepperMotorHardwareMode);

        }

        void IDisposable.Dispose()
        {
            _lcdController.Dispose();
            _ledController.Dispose();
            //_azElController.Dispose();  //TODO:  ? ? ? ? ? ? ?
        }

        public async Task StartTrackingAsync()
        {
            Console.WriteLine("Starting flight tracking...");

            Task task = _nearestAirCraftDetector.StartTrackingAsync();

            _lcdController.Initialize();
            _lcdController.Blank();

            _ledController.Initialize();
            _ledController.SetFlightTrackerState(FlightTrackerState.NoAirCraftFound);

            string? formerIcao = null;
            while (true)
            {
                DateTime loopStart = DateTime.Now;
                var nearestAirCraftResult = _nearestAirCraftDetector.GetNearestAirCraft();
                if (nearestAirCraftResult != null)
                {
                    //-- fisrst aircraft found
                    if (formerIcao == null)
                    {
                        double azimuth = nearestAirCraftResult.AircraftAzElPosition.Azimuth;
                        double elevation = nearestAirCraftResult.AircraftAzElPosition.Elevation;
                        _lcdController.ApproachingTarget(azimuth, elevation);
                        _ledController.SetFlightTrackerState(FlightTrackerState.MovingToAirCraft);
                        _azElController.MoveToAzEl(azimuth, elevation);
                    }
                    else
                    {
                        if (formerIcao == nearestAirCraftResult.AircraftTrack.Icao)
                        {
                            //-- same aircraft, update position
                            double durationSec = 1;
                            double azimuth = nearestAirCraftResult.AircraftAzElPosition.Azimuth;
                            double elevation = nearestAirCraftResult.AircraftAzElPosition.Elevation;
                            _lcdController.ApproachingTarget(azimuth, elevation);  //TODO: Sludder??????
                            _ledController.SetFlightTrackerState(FlightTrackerState.TrackingAirCraft);
                            _azElController.MoveToAzEl(azimuth, elevation, durationSec);
                        }
                        else
                        {
                            //-- new aircraft detected
                            double azimuth = nearestAirCraftResult.AircraftAzElPosition.Azimuth;
                            double elevation = nearestAirCraftResult.AircraftAzElPosition.Elevation;
                            _lcdController.ApproachingTarget(azimuth, elevation);
                            _ledController.SetFlightTrackerState(FlightTrackerState.MovingToAirCraft);
                            _azElController.MoveToAzEl(azimuth, elevation);
                        }
                    }
                }
                else
                {
                    //-- no aircraft found
                    _lcdController.NoTracks();
                    _ledController.SetFlightTrackerState(FlightTrackerState.NoAirCraftFound);
                    _azElController.MoveToAzEl(0, 0);
                }

                DateTime loopEnd = DateTime.Now;
                TimeSpan loopDuration = loopEnd - loopStart;
                if (loopDuration.TotalMilliseconds > 1000)
                {
                    double delayMilliSec = (loopDuration.TotalMilliseconds - 1000);
                    await Task.Delay((int)delayMilliSec); 
                }
            }
        }
    }
}
