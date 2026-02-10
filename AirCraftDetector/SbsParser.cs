using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirCraftDetector
{
    public static class SbsParser
    {
        public static SbsMessage Parse(string line)
        {
            var parts = line.Split(',');

            if (parts.Length < 22)
                return null;

            return new SbsMessage
            {
                MessageType = parts[0],
                TransmissionType = parts[1],
                Icao = parts[4],
                Callsign = Empty(parts[10]),
                Altitude = Int(parts[11]),
                GroundSpeed = Double(parts[12]),
                Track = Double(parts[13]),
                Latitude = Double(parts[14]),
                Longitude = Double(parts[15]),
                VerticalRate = Int(parts[16]),
                OnGround = Bool(parts[21]),
                Timestamp = ParseDate(parts[6], parts[7])
            };
        }

        static int? Int(string s) => int.TryParse(s, out var v) ? v : null;

        static double? Double(string s) =>
            double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : null;

        static bool? Bool(string s) => s == "1" ? true : s == "0" ? false : null;

        static string Empty(string s) => string.IsNullOrWhiteSpace(s) ? null : s;

        static DateTime? ParseDate(string date, string time)
        {
            if (DateTime.TryParseExact(
                $"{date} {time}",
                "yyyy/MM/dd HH:mm:ss.fff",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal,
                out var dt))
                return dt;

            return null;
        }
    }

}
