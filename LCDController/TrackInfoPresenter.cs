using Iot.Device.CharacterLcd;
using Iot.Device.Nmea0183.Sentences;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using HardwareMode;

namespace LCDController
{
    public class TrackInfoPresenter : IDisposable
    {
        private readonly LCD20x4Controller _lcd20x4Controller;
        private CancellationTokenSource? _cts;
        private readonly string _degreeSymbol; 

        private Task? _task;

        public TrackInfoPresenter(HardwareModeEnum mode)
        {
            _cts = null;
            _task = null;
            _lcd20x4Controller = new LCD20x4Controller(mode);

            if (mode == HardwareModeEnum.Real)
                _degreeSymbol = "\xDF"; // Degree symbol for LCD display (custom character)
            else
                _degreeSymbol = "°"; // Regular degree symbol for console output
        }

        /// <summary>
        /// Initializes the LCD display controller.
        /// </summary>
        public void Initialize()
        {
            _lcd20x4Controller.Initialize();
        }

        public void Dispose()
        {
            _CancelTrackingTask();
            _lcd20x4Controller.Dispose();
        }


        public void Blank()
        {
            _CancelTrackingTask();
            string[] lines = new string[] { "", "", "", "" };
            _lcd20x4Controller.WriteDisplay(lines);
        }


        public void NoTracks()
        {
            _CancelTrackingTask();
            string[] lines = new string[] { "", "     No tracks", "", "" };
            _lcd20x4Controller.WriteDisplay(lines);
        }

        public void ApproachingTarget(double az, double el)
        {
            _CancelTrackingTask();
            string compassDir = GetCompassDirection(az);

            string line0 = "Approaching position";
            string line1 = string.Create(CultureInfo.InvariantCulture, $"AZ: {az,8:F1} {_degreeSymbol} ({compassDir})");
            string line2 = string.Create(CultureInfo.InvariantCulture, $"EL: {el,8:F1} {_degreeSymbol}");
            string line3 = "";

            _lcd20x4Controller.WriteDisplay(new string[] { line0, line1, line2, line3 });
        }

        public void AircraftTracking(double az, double el, double altMeter, double dist, string callsign, string icao)
        {
            _CancelTrackingTask();
            
            // Opret CTS INDEN task startes
            _cts = new CancellationTokenSource();
            
            _task = Task.Run(() => AircraftTrackingAsync(az, el, altMeter, dist, callsign, icao, _cts.Token));
        }

        public async Task AircraftTrackingAsync(double az, double el, double altMeter, double dist, string callsign, string icao, CancellationToken token)
        {
            string compassDir = GetCompassDirection(az);

            string posLine0 = string.Create(CultureInfo.InvariantCulture, $"AZ  :{az,8:N1} {_degreeSymbol} ({compassDir})");
            string posLine1 = string.Create(CultureInfo.InvariantCulture, $"EL  :{el,8:N1} {_degreeSymbol}");
            string posLine2 = string.Create(CultureInfo.InvariantCulture, $"ALT :{altMeter,8:N1} m");
            string posLine3 = string.Create(CultureInfo.InvariantCulture, $"DIST:{dist,8:N1} m");
            string[] posInfoLines = new string[] { posLine0, posLine1, posLine2, posLine3 };

            string flightInfoLine0 = string.Create(CultureInfo.InvariantCulture, $"Callsign: {callsign}");
            string flightInfoLine1 = string.Create(CultureInfo.InvariantCulture, $"Icao    : {icao}");
            string flightInfoLine2 = string.Empty;
            string flightInfoLine3 = string.Empty;
            string[] flightInfoLines = new string[] { flightInfoLine0, flightInfoLine1, flightInfoLine2, flightInfoLine3 };

            bool showPosInfo = true;
            while (!token.IsCancellationRequested)  // Check token
            {
                try
                {
                    if (showPosInfo)
                        _lcd20x4Controller.WriteDisplay(posInfoLines);
                    else
                        _lcd20x4Controller.WriteDisplay(flightInfoLines);
                    
                    await Task.Delay(showPosInfo ? 2000 : 1000, token);  // Use await
                    showPosInfo = !showPosInfo;
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
            _cts = null;
        }


        private void _CancelTrackingTask()
        {
            if (_task != null)
            {
                //Console.WriteLine("Cancelling Task...");
                _cts?.Cancel();
                int counter = 0;
                while (!_task.IsCompleted)
                {
                    //Console.WriteLine("Waiting for Task to complete...");
                    Thread.Sleep(10); // Small delay to ensure the previous tracking task has been cancelled before starting a new one
                    counter++;
                    if (counter > 100) // Timeout after 1 second
                    {
                        throw new TimeoutException("Task cancellation timeout. The tracking task did not complete within the expected time frame.");
                    }
                }
                //Console.WriteLine("Task cancelled and completed.");
                _task = null;
            }
        }



        /// <summary>
        /// Convert azimuth to compass direction text (8 directions)
        /// </summary>
        /// <param name="azimuth">Azimuth in degrees (0-360)</param>
        /// <returns>Compass direction (N, NE, E, SE, S, SW, W, NW)</returns>
        private static string GetCompassDirection(double azimuth)
        {
            // Normalize to 0-360 range
            azimuth = (azimuth % 360 + 360) % 360;

            // 8 main directions (45° sectors)
            string[] directions = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
            int index = (int)Math.Round(azimuth / 45.0) % 8;
            
            return directions[index];
        }
    }
}
