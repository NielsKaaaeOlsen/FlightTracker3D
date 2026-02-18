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
    // FlighttrackInfoLcdPresenter  or FlightTRackerLCD20x4Presenter
    public class FlightTRackerLCD20x4Controller : IDisposable
    {
        private readonly LCD20x4Controller _lcd20x4Controller;
        public FlightTRackerLCD20x4Controller(LcdHardwareMode mode)
        {
            _lcd20x4Controller = new LCD20x4Controller(mode);
        }

        public void Initialize()
        {
            _lcd20x4Controller.Initialize();
        }

        public void Dispose()
        {
            _lcd20x4Controller.Dispose();
        }

        public void NoTracks()
        {
            string[] lines = new string[] { "", "     No tracks", "", "" };
            _lcd20x4Controller.WriteDisplay(lines);
        }

        public void ApproachingTarget(double az, double el)//, double altMeter)  //ApproachingPosition ?
        {
            string compassDir = GetCompassDirection(az);

            string line0 = "Approaching position";
            string line1 = string.Create(CultureInfo.InvariantCulture, $"AZ: {az,8:F1}° ({compassDir})");
            string line2 = string.Create(CultureInfo.InvariantCulture, $"EL: {el,8:F1}°");
            string line3 = "";

            _lcd20x4Controller.WriteDisplay(new string[] { line0, line1, line2, line3 });
        }


        /*
#2
----X----X----X----X
AZ  :   123.4° (SE)
EL  :   123.4°
ALT : 12345.1m
DIST: 12345.1m         */

        public void Repositioning(double az, double el, double altMeter, double dist, string callsign, string icao)
        {
            string compassDir = GetCompassDirection(az);

            string line0 = string.Create(CultureInfo.InvariantCulture, $"AZ  :{az,8:F1}° ({compassDir})");
            string line1 = string.Create(CultureInfo.InvariantCulture, $"EL  :{el,8:F1}°");
            string line2 = string.Create(CultureInfo.InvariantCulture, $"ALT :{altMeter,8:F1}m");  //TODO: F1 --> N1 
            string line3 = string.Create(CultureInfo.InvariantCulture, $"DIST:{dist,8:F1}m");

            _lcd20x4Controller.WriteDisplay(new string[] { line0, line1, line2, line3 });


            /*
FlightTrackerLcdPresenter 
•	✅ Følger MVP (Model-View-Presenter) pattern
•	✅ Klar rolle: Formaterer og præsenterer flight data til LCD
•	✅ Adskiller presentation logic fra hardware control
Andre gode alternativer:
Pattern-baseret:
•	FlightTrackerLcdAdapter — Adapter pattern (konverterer flight data til LCD format)
•	FlightTrackerLcdFacade — Facade pattern (forenkler kompleks LCD API)
•	FlightTrackerLcdFormatter — Fokuserer på formatering
Display-orienteret:
•	FlightTrackerDisplay — Simpelt, generisk
•	FlightTrackerLcdView — View layer i MVC/MVP
•	FlightTrackerDisplay20x4 — Inkluderer display størrelse
Kontekst-specifik:
•	AircraftTrackingDisplay — Beskriver hvad der vises
•	FlightInfoDisplay — Fokuserer på information type
*/
            //TODO: Callsign, icao, from to
            //Task 

            //CODE #1
            //// Opret CancellationTokenSource
            //var cts = new CancellationTokenSource();

            //// Start task med token
            //var task = Task.Run(() => DoWorkAsync(cts.Token));

            //// Cancel efter 5 sekunder
            //await Task.Delay(5000);
            //cts.Cancel();

            //// Await task completion
            //await task;

            //CODE #2
            //await Task.Delay(10000, cancellationToken);

            //// Brug:
            //var cts = new CancellationTokenSource();
            //try
            //{
            //    await Task.Delay(10000, cts.Token);
            //}
            //catch (OperationCanceledException)
            //{
            //    Console.WriteLine("Delay cancelled!");
            //}

            // Cancel fra anden tråd:
            //cts.Cancel();  // Hopper ud af Task.Delay med det samme

            //Pickup from to from Web

            //Move to "Park position""
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

        /*
                   x
SAS1080 KLMN->CPH
AZ: 123.4' (NW) EL: 12.1'
ALT: 12123 M 
DIST: 1234.561 KM


#2
AZ: 123.4' (NW) 
EL: 12.1'
ALT: 12.123 KM 
DIST: 1234.561 KM


#1
Callsign: SAS1080 
Icao: 0xjsdjedw
KLMN:  Amsterdam
CPH:  Copenhagen zzzz

--------------------------
#1
No tracks

__________________________

#1
Moving to new target:
AZ: 123.4' (NW) 
EL: 12.1'
ALT: 12.123 KM 

----X----X----X----X
Moving to new target
AZ: 11.1° (N)
EL: 22.2°
ALT: 12345.6 m


*/

    }
}
