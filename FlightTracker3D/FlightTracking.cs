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
            //-- Determine host based on hardware mode for AirCraftListener. 
            //   If in Real mode, use the IP address of the Raspberry PI running the ReadSr service. 
            //   If in Simulation mode, use localhost and execute the MockSbsServer. 
            string host = hardwareModes.AirCraftListenerMode == HardwareModeEnum.Real ? "192.168.1.92" : "localhost";
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

            _azElController.SetMicrostepping(MicrosteppingMode.M8);  //TODO: make this configurable
        }

        void IDisposable.Dispose()
        {
            _lcdController.Dispose();
            _ledController.Dispose();
            _azElController.Dispose();
        }

        public async Task StartTrackingAsync()
        {
            Console.WriteLine("Starting flight tracking...");

            Task task = _nearestAirCraftDetector.StartTrackingAsync();

            _lcdController.Initialize();
            _lcdController.Blank();

            _ledController.Initialize();
            _ledController.SetFlightTrackerState(FlightTrackerState.NoAirCraftFound);

            _azElController.Initialize();

            string? formerIcao = null;
            while (true)
            {
                DateTime loopStart = DateTime.Now;
                var nearestAirCraftResult = _nearestAirCraftDetector.GetNearestAirCraft();
                if (nearestAirCraftResult != null)
                {
                    if (formerIcao == null)
                    {
                        //-- fisrst aircraft found
                        Console.WriteLine($"First aircraft found: ICAO={nearestAirCraftResult.AircraftTrack.Icao}, Callsign={nearestAirCraftResult.AircraftTrack.Callsign}");
                        double azimuth = nearestAirCraftResult.AircraftAzElPosition.Azimuth;
                        double elevation = nearestAirCraftResult.AircraftAzElPosition.Elevation;
                        _lcdController.ApproachingTarget(azimuth, elevation);
                        _ledController.SetFlightTrackerState(FlightTrackerState.MovingToAirCraft);
                        await _azElController.MoveToAsync(azimuth, elevation);
                        formerIcao = nearestAirCraftResult.AircraftTrack.Icao;
                    }
                    else
                    {
                        if (formerIcao == nearestAirCraftResult.AircraftTrack.Icao)
                        {
                            //-- same aircraft, update position only
                            Console.WriteLine($"Tracking same aircraft: ICAO={nearestAirCraftResult.AircraftTrack.Icao}, Callsign={nearestAirCraftResult.AircraftTrack.Callsign}");
                            double durationSec = 3; //TODO: make this configurable
                            string callsign = nearestAirCraftResult.AircraftTrack.Callsign;
                            string icao = nearestAirCraftResult.AircraftTrack.Icao;
                            double azimuth = nearestAirCraftResult.AircraftAzElPosition.Azimuth;
                            double elevation = nearestAirCraftResult.AircraftAzElPosition.Elevation;
                            double distanceMeter = nearestAirCraftResult.AircraftAzElPosition.Distance;
                            double altitudeMeter = nearestAirCraftResult.AircraftTrack.History.Last<PositionPoint>().AltitudeMeters;
                            _lcdController.AircraftTracking(azimuth, elevation, altitudeMeter, distanceMeter, callsign, icao);
                            _ledController.SetFlightTrackerState(FlightTrackerState.TrackingAirCraft);
                            await _azElController.MoveToAsync(azimuth, elevation, durationSec);
                        }
                        else
                        {
                            //-- new aircraft detected
                            Console.WriteLine($"New aircraft detected: ICAO={nearestAirCraftResult.AircraftTrack.Icao}, Callsign={nearestAirCraftResult.AircraftTrack.Callsign}");
                            double azimuth = nearestAirCraftResult.AircraftAzElPosition.Azimuth;
                            double elevation = nearestAirCraftResult.AircraftAzElPosition.Elevation;
                            _lcdController.ApproachingTarget(azimuth, elevation);
                            _ledController.SetFlightTrackerState(FlightTrackerState.MovingToAirCraft);
                            await _azElController.MoveToAsync(azimuth, elevation);
                            formerIcao = nearestAirCraftResult.AircraftTrack.Icao;
                        }
                    }
                }
                else
                {
                    //-- no aircraft found
                    Console.WriteLine("No aircraft found.");
                    _lcdController.NoTracks();
                    _ledController.SetFlightTrackerState(FlightTrackerState.NoAirCraftFound);
                    await _azElController.MoveToAsync(0, 0);
                }

                DateTime loopEnd = DateTime.Now;
                TimeSpan loopDuration = loopEnd - loopStart;
                if (loopDuration.TotalMilliseconds < 3000) //TODO: make this configurable
                {
                    double delayMilliSec = (3000 - loopDuration.TotalMilliseconds);
                    await Task.Delay((int)delayMilliSec); 
                }
            }
        }
    }
}
