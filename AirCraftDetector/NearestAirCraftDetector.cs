using GeoUtil;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirCraftDetector
{
    public class NearestAirCraftDetector
    {
        private readonly ConcurrentDictionary<string, AircraftTrack> _tracks;
        private readonly AirCraftListener _airCraftListener;
        private readonly PositionPoint _referencePoint;

        private readonly object _tracksLock = new();

        public NearestAirCraftDetector(string host, int port, PositionPoint referencePoint)
        {
            _tracks = new ConcurrentDictionary<string, AircraftTrack>();
            _airCraftListener = new AirCraftListener(host, port, _tracks, _tracksLock);

            _referencePoint = referencePoint;
        }

        public async Task StartTrackingAsync()
        {
            Task task = Task.Run(() => _airCraftListener.ListenForAircraftAsync());

            Task taskCleanup = Task.Run(() => _airCraftListener.RemoveExpiredTracks());
        }


        public NearestAirCraftDetectorResult?  GetNearestAirCraftAndForecastPosition(DateTime tnew)
        {
            double distMin = Double.MaxValue;
            AircraftTrack? bestTrack = null;
            lock (_tracksLock)
            {
                if (_tracks.Values.Count == 0)
                {
                    return null;  // No aircrafts found
                }
                foreach (var track in _tracks.Values)
                {
                    if (track.History.Count == 0) continue;  // Skip tomme tracks
                    var trackPos = track.History.Last<PositionPoint>();

                    double distance = _referencePoint.DistanceTo(trackPos);
                    if (distance < distMin)
                    {
                        bestTrack = track;
                        distMin = distance;
                    }
                }
                if (bestTrack != null)
                {
                    PositionPoint forecasetPos = bestTrack.GetForecastPosition(tnew);
                    AzElPosition azElPos = _referencePoint.ToAzElPosition(forecasetPos);
                    var detectorResult = new NearestAirCraftDetectorResult(bestTrack, forecasetPos, azElPos);

                    if (azElPos.Azimuth < 0 || azElPos.Azimuth >= 360)
                        throw new ArgumentOutOfRangeException(nameof(azElPos.Azimuth), "Azimuth degrees must be between 0 and 360.");
                    if (azElPos.Elevation < -10 || azElPos.Elevation > 90) // Allowing a bit below the horizon for aircrafts that are just below the horizon
                        throw new ArgumentOutOfRangeException(nameof(azElPos.Elevation), $"Elevation degrees must be between -10 and 90:  {azElPos.Elevation}");

                    return detectorResult;
                }
                else { return null; }
            }
        }

        public void SetFakeTracksForTesting(List<AircraftTrack> tracks)
        {
            foreach (var track in tracks)
            {
                _tracks[track.Icao] = track;
            }
        }
    }
}
