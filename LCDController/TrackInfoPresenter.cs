using Iot.Device.CharacterLcd;
using Iot.Device.Nmea0183.Sentences;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCDController
{
    public class TrackInfoPresenter : IDisposable
    {
        private readonly LCD20x4Controller _lcd20x4Controller;
        private CancellationTokenSource? _cts;

        public TrackInfoPresenter(LcdHardwareMode mode)
        {
            _cts = null;
            _lcd20x4Controller = new LCD20x4Controller(mode);
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
            _cts?.Cancel();
            _lcd20x4Controller.Dispose();
        }

        public void NoTracks()
        {
            _cts?.Cancel();
            string[] lines = new string[] { "", "     No tracks", "", "" };
            _lcd20x4Controller.WriteDisplay(lines);
        }

        public void ApproachingTarget(double az, double el)
        {
            _cts?.Cancel();
            string compassDir = GetCompassDirection(az);

            string line0 = "Approaching position";
            string line1 = string.Create(CultureInfo.InvariantCulture, $"AZ: {az,8:F1}° ({compassDir})");
            string line2 = string.Create(CultureInfo.InvariantCulture, $"EL: {el,8:F1}°");
            string line3 = "";

            _lcd20x4Controller.WriteDisplay(new string[] { line0, line1, line2, line3 });
        }

        public void AircraftTracking(double az, double el, double altMeter, double dist, string callsign, string icao)
        {
            _cts?.Cancel();
            Task task = Task.Run(() => AircraftTrackingAsync(az: az, el: el, altMeter: altMeter, dist: dist, callsign: callsign, icao: icao));
        }

        public async Task AircraftTrackingAsync(double az, double el, double altMeter, double dist, string callsign, string icao)
        {
            string compassDir = GetCompassDirection(az);

            string posLine0 = string.Create(CultureInfo.InvariantCulture, $"AZ  :{az,8:N1}° ({compassDir})");
            string posLine1 = string.Create(CultureInfo.InvariantCulture, $"EL  :{el,8:N1}°");
            string posLine2 = string.Create(CultureInfo.InvariantCulture, $"ALT :{altMeter,8:N1}m");  //TODO: F1 --> N1 
            string posLine3 = string.Create(CultureInfo.InvariantCulture, $"DIST:{dist,8:N1}m");
            string[] posInfoLines = new string[] { posLine0, posLine1, posLine2, posLine3 };

            string flightInfoLine0 = string.Create(CultureInfo.InvariantCulture, $"Callsign: {callsign}");
            string flightInfoLine1 = string.Create(CultureInfo.InvariantCulture, $"Icao    : {icao}");
            string flightInfoLine2 = string.Empty;
            string flightInfoLine3 = string.Empty;
            string[] flightInfoLines = new string[] { flightInfoLine0, flightInfoLine1, flightInfoLine2, flightInfoLine3 };

            // Create CancellationTokenSource
            _cts = new CancellationTokenSource();
            while (true)
            {
                try
                {
                    _lcd20x4Controller.WriteDisplay(posInfoLines);
                    Task.Delay(2000, _cts.Token).GetAwaiter().GetResult();
                    _lcd20x4Controller.WriteDisplay(flightInfoLines);
                    Task.Delay(1000, _cts.Token).GetAwaiter().GetResult();
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Delay cancelled!");
                    break;
                }
            }
            _cts = null;

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
