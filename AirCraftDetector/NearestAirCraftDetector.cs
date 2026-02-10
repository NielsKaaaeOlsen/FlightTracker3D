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

        public NearestAirCraftDetector()
        {
            _tracks = new ConcurrentDictionary<string, AircraftTrack>();

            _airCraftListener = new AirCraftListener(_tracks);
        }

        public async Task Initialize()
        {
            Task task = Task.Run(() => _airCraftListener.Listen());
        }


        public NearestAirCraftDetectorResult  GetNearestAirCraft()
        {
            double distMin = Double.MaxValue;
            AircraftTrack best = null;   //TODO:
            lock (_tracks)
            {
                foreach (var track in _tracks.Values)   //Test if empty
                {
                    var pos = track.History.Last<PositionPoint>();
                    var dist = pos.Latitude + pos.Longitude;
                    if (dist < distMin)  // TODO:
                    {
                        best = track;
                        distMin = dist;
                    }
                }
            }
            var nearest = new NearestAirCraftDetectorResult();
            nearest.Type = NearestAirCraftDetectorResult.ResultType.newAircraftDetected;  //TODO:
            nearest.AircraftTrack = best;
            return nearest;
        }
    }
}
