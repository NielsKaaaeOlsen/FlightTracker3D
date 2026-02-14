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

        public NearestAirCraftDetector(string host, int port, PositionPoint referencePoint)
        {
            _tracks = new ConcurrentDictionary<string, AircraftTrack>();
            _airCraftListener = new AirCraftListener(host, port, _tracks);

            _referencePoint = referencePoint;
        }

        public async Task Initialize()
        {
            Task task = Task.Run(() => _airCraftListener.Listen());
        }


        public NearestAirCraftDetectorResult?  GetNearestAirCraft()
        {
            double distMin = Double.MaxValue;
            AircraftTrack? bestTrack = null; 
            lock (_tracks)
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
            }
            //TODO: Determine if this is a new aircraft or an existing one. For now, we will assume it's a new aircraft. :-)
            // We could keep track of the last detected aircraft and compare it with the bestTrack to determine if it's new or existing. :-)
            // 
            var nearest = new NearestAirCraftDetectorResult();
            nearest.Type = NearestAirCraftDetectorResult.ResultType.newAircraftDetected;  //TODO:
            nearest.AircraftTrack = bestTrack;
            return nearest;
        }
    }
}
