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

        public FlightTracking(HardwareModes hardwareModes, MicrosteppingMode microsteppingMode, ILoggerFactory? loggerFactory)
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
                AltitudeFeet = 150   // ~ 50 meters above sea level
            };

            _nearestAirCraftDetector = new NearestAirCraftDetector(host, port, referencePoint);
            _lcdController = new TrackInfoPresenter(hardwareModes.LcdHardwareMode);
            _ledController = new FlightTrackerLEDController(hardwareModes.LedHardwareMode);
            _azElController = new AzElController(loggerFactory, hardwareModes.StepperMotorHardwareMode);

            _azElController.SetMicrostepping(microsteppingMode);
        }

        void IDisposable.Dispose()
        {
            _lcdController.Dispose();
            _ledController.Dispose();
            _azElController.Dispose();
        }

        /// <summary>
        /// Performs graceful shutdown by moving stepper motors to home position (0, 0).
        /// </summary>
        private async Task ShutdownAsync()
        {
            Console.WriteLine("Shutting down: Moving stepper motors to home position (0, 0)...");
            _lcdController.Blank();
            _ledController.SetFlightTrackerState(FlightTrackerState.NoAirCraftFound);
            await _azElController.MoveToAsync(0, 0);

            (int azStep, int elStep) = _azElController.GetCurrentAzElStep();
            Console.WriteLine($"Stepper motors at home position:  azStep={azStep}, elStep={elStep} \nAzEl controller shutdown complete.", azStep, elStep);
        }

        public async Task StartTrackingAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                Console.WriteLine("Starting flight tracking...");

                Task task = _nearestAirCraftDetector.StartTrackingAsync();

                _lcdController.Initialize();
                _lcdController.Blank();

                _ledController.Initialize();
                _ledController.SetFlightTrackerState(FlightTrackerState.NoAirCraftFound);

                _azElController.Initialize();

                TimeSpan durationLoop = TimeSpan.FromSeconds(4); //TODO: make this configurable

                NearestAirCraftDetectorResult? formerNearestAirCraftResult = null;
                while (!cancellationToken.IsCancellationRequested)
                {
                    DateTime loopStart = DateTime.UtcNow;
                    Console.WriteLine("--------------------------------------------------");
                    Console.WriteLine($"Loop start: {loopStart:HH:mm:ss.fff}");

                    //-- Before moving the stepper motors, check if the current step motor position matches the expected forecast position from the last forecast position. If they differ by more than a certain threshold, throw an exception.
                    if (formerNearestAirCraftResult != null)
                        CheckStepperMotorMovement(formerNearestAirCraftResult.AircraftAzElForecastPosition);

                    var nearestAirCraftResult = _nearestAirCraftDetector.GetNearestAirCraftAndForecastPosition(loopStart + durationLoop);
                    if (nearestAirCraftResult != null)
                    {
                        if (formerNearestAirCraftResult == null)
                        {
                            //-- fisrst aircraft found
                            Console.WriteLine($"First aircraft found: ICAO={nearestAirCraftResult.AircraftTrack.Icao}, Callsign={nearestAirCraftResult.AircraftTrack.Callsign} Forecast time: {nearestAirCraftResult.AircraftForecastPosition.Timestamp}");
                            double azimuth = nearestAirCraftResult.AircraftAzElForecastPosition.Azimuth;
                            double elevation = nearestAirCraftResult.AircraftAzElForecastPosition.Elevation;
                            _lcdController.ApproachingTarget(azimuth, elevation);
                            _ledController.SetFlightTrackerState(FlightTrackerState.MovingToAirCraft);
                            await _azElController.MoveToAsync(azimuth, elevation);
                            formerNearestAirCraftResult = nearestAirCraftResult;
                        }
                        else
                        {
                            if (formerNearestAirCraftResult.AircraftTrack.Icao == nearestAirCraftResult.AircraftTrack.Icao)
                            {
                                //-- same aircraft, update position only
                                Console.WriteLine($"Tracking same aircraft: ICAO={nearestAirCraftResult.AircraftTrack.Icao}, Callsign={nearestAirCraftResult.AircraftTrack.Callsign} Forecast time: {nearestAirCraftResult.AircraftForecastPosition.Timestamp} Forecast distance: {nearestAirCraftResult.AircraftAzElForecastPosition.Distance}");
                                string callsign = nearestAirCraftResult.AircraftTrack.Callsign;
                                string icao = nearestAirCraftResult.AircraftTrack.Icao;
                                double azimuth = nearestAirCraftResult.AircraftAzElForecastPosition.Azimuth;
                                double elevation = nearestAirCraftResult.AircraftAzElForecastPosition.Elevation;
                                double distanceMeter = nearestAirCraftResult.AircraftAzElForecastPosition.Distance;
                                double altitudeMeter = nearestAirCraftResult.AircraftForecastPosition.AltitudeMeters; //AircraftTrack.History.Last<PositionPoint>().AltitudeMeters;

                                double azimuthDiff = Math.Abs(azimuth - formerNearestAirCraftResult.AircraftAzElForecastPosition.Azimuth);
                                if (azimuthDiff < 180)
                                {
                                    //--  small azimuth difference, move to new position with duration specified (slow movement)
                                    _lcdController.AircraftTracking(azimuth, elevation, altitudeMeter, distanceMeter, callsign, icao);
                                    _ledController.SetFlightTrackerState(FlightTrackerState.TrackingAirCraft);
                                    await _azElController.MoveToAsync(azimuth, elevation, durationLoop.TotalSeconds);
                                }
                                else
                                {
                                    //-- Large azimuth difference, move to new position without duration (fast movement)
                                    Console.WriteLine($"Azimuth difference too large: {azimuthDiff} degrees. Fast movement to new position");
                                    _lcdController.AircraftTracking(azimuth, elevation, altitudeMeter, distanceMeter, callsign, icao);
                                    _ledController.SetFlightTrackerState(FlightTrackerState.MovingToAirCraft);
                                    await _azElController.MoveToAsync(azimuth, elevation);

                                }
                                formerNearestAirCraftResult = nearestAirCraftResult;
                            }
                            else
                            {
                                //-- new aircraft detected
                                Console.WriteLine($"New aircraft detected: ICAO={nearestAirCraftResult.AircraftTrack.Icao}, Callsign={nearestAirCraftResult.AircraftTrack.Callsign} Forecast time: {nearestAirCraftResult.AircraftForecastPosition.Timestamp}");
                                double azimuth = nearestAirCraftResult.AircraftAzElForecastPosition.Azimuth;
                                double elevation = nearestAirCraftResult.AircraftAzElForecastPosition.Elevation;
                                _lcdController.ApproachingTarget(azimuth, elevation);
                                _ledController.SetFlightTrackerState(FlightTrackerState.MovingToAirCraft);
                                await _azElController.MoveToAsync(azimuth, elevation);
                                formerNearestAirCraftResult = nearestAirCraftResult;
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
                        formerNearestAirCraftResult = null;
                    }

                    DateTime loopEnd = DateTime.UtcNow;
                    TimeSpan actualDuration = loopEnd - loopStart;
                    TimeSpan allowedGap = TimeSpan.FromSeconds(1);
                    if (actualDuration < durationLoop - allowedGap)
                    {
                        double delayMilliSec = (durationLoop.TotalMilliseconds - actualDuration.TotalMilliseconds);
                        try
                        {
                            await Task.Delay((int)delayMilliSec, cancellationToken);
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                    }
                }

                //-- Graceful shutdown
                await ShutdownAsync();
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Flight tracking cancelled.");
                await ShutdownAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in flight tracking: {ex.Message}");
                _lcdController.ShowException(ex);

                string filePath = $"Exception_{DateTime.UtcNow:yyyyMMdd_HHmmss}.txt";
                System.IO.File.WriteAllText(filePath, ex.ToString());
                return;
            }
        }

        private void CheckStepperMotorMovement(AzElPosition expectedPosition)
        {
            //-- Compare the current stepper motor position from AzElController and stepMotor controller with the expected position and throw an exception if they differ by more than a certain threshold.

            //-- From the controller get the current stepper motor position in steps and convert to degrees
            (int azStepController, int elStepController) = _azElController.GetCurrentAzElStep();
            (double azDegPerStep, double elDegPerStep) = _azElController.GetDegreesPerStep();
            double currentAzimuthController = azStepController * azDegPerStep;
            double currentElevationController = elStepController * elDegPerStep;

            //-- Calculate the difference between the current controller position and the expected position
            double azimuthDiffController = Math.Abs(currentAzimuthController - expectedPosition.Azimuth);
            double elevationDiffController = Math.Abs(currentElevationController - expectedPosition.Elevation);

            //-- Get the accumulated stepper motor movement from step motors and convert to degrees
            (int azStepMotor, int elStepMotor) = _azElController.GetAccumulatedStepMotorMovement();
            double azimuthDiffMotor = Math.Abs(azStepMotor * azDegPerStep - expectedPosition.Azimuth);
            double elevationDiffMotor = Math.Abs(elStepMotor * elDegPerStep - expectedPosition.Elevation);

            if (azimuthDiffMotor > azDegPerStep || elevationDiffMotor > elDegPerStep || 
                azimuthDiffController > azDegPerStep || elevationDiffController > elDegPerStep)
            {
                string failure =
                    "Error: Stepper motor position differs from expected position." +
                     $"Expected: (az={expectedPosition.Azimuth}, el={expectedPosition.Elevation})" +
                     $"Current (controller): (az={currentAzimuthController}, el={currentElevationController}) " +
                     $"Current (stepmotor): (az={azStepMotor * azDegPerStep}, el={elStepMotor * elDegPerStep}) ";
                Console.WriteLine(failure);
                throw new Exception(failure);
            }
        }
    }
}
