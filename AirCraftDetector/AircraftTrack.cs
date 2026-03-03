using GeoUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirCraftDetector
{
    public class AircraftTrack
    {
        public string Icao { get; set; }
        public string Callsign { get; set; }

        public List<PositionPoint> History { get; } = new();
        public DateTime LastSeen { get; set; }

        public PositionPoint GetForecastPosition(DateTime tnew)
        {
            if (History.Count == 0)
                throw new Exception("No historic data found. Can't calculate forecast position.");
            else if (History.Count == 1)
                return History[0];
            else if (History.Count == 2)
            {
                if (History[0].Timestamp == History[1].Timestamp) return History[1];

                (double X0, double Y0, double Z0) = GeoDistance.GeodeticToEcef(History[0].Latitude, History[0].Longitude, History[0].AltitudeMeters);
                (double X1, double Y1, double Z1) = GeoDistance.GeodeticToEcef(History[1].Latitude, History[1].Longitude, History[1].AltitudeMeters);

                Vec3 P0 = new Vec3(X0, Y0, Z0);
                Vec3 P1 = new Vec3(X1, Y1, Z1);
                Vec3 Pnew = InterpolateVec3.LinearInterpolate(History[0].Timestamp, P0, History[1].Timestamp, P1, tnew);

                (double LatDeg, double LonDeg, double AltMeters) = GeoDistance.EcefToGeodetic(Pnew.X, Pnew.Y, Pnew.Z);
               
                return new PositionPoint() { Timestamp = tnew, Latitude = LatDeg, Longitude = LonDeg, Altitude = (int)(AltMeters / 0.3048) };
            }
            else
            {
                int ih0 = History.Count - 1;
                int ih1 = History.Count - 2;
                int ih2 = History.Count - 3;

                if (History[ih0].Timestamp == History[ih1].Timestamp
                 || History[ih1].Timestamp == History[ih2].Timestamp) return History[ih0];

                (double X0, double Y0, double Z0) = GeoDistance.GeodeticToEcef(History[ih0].Latitude, History[ih0].Longitude, History[ih0].AltitudeMeters);
                (double X1, double Y1, double Z1) = GeoDistance.GeodeticToEcef(History[ih1].Latitude, History[ih1].Longitude, History[ih1].AltitudeMeters);
                (double X2, double Y2, double Z2) = GeoDistance.GeodeticToEcef(History[ih2].Latitude, History[ih2].Longitude, History[ih2].AltitudeMeters);

                Vec3 P0 = new Vec3(X0, Y0, Z0);
                Vec3 P1 = new Vec3(X1, Y1, Z1);
                Vec3 P2 = new Vec3(X2, Y2, Z2);
                Vec3 Pnew = InterpolateVec3.QuadraticInterpolate(History[ih0].Timestamp, P0, History[ih1].Timestamp, P1, History[ih2].Timestamp, P2, tnew);

                (double LatDeg, double LonDeg, double AltMeters) = GeoDistance.EcefToGeodetic(Pnew.X, Pnew.Y, Pnew.Z);

                return new PositionPoint() { Timestamp = tnew, Latitude = LatDeg, Longitude = LonDeg, Altitude = (int)(AltMeters / 0.3048) };
            }
        }
    }

}
