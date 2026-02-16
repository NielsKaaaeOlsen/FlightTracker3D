using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirCraftDetector
{
    public class AirCraftListener
    {
        private readonly ConcurrentDictionary<string, AircraftTrack> _tracks;

        private readonly int _port;
        private readonly string _host;

        private readonly int _timeoutSec;
        public AirCraftListener(string host, int port, ConcurrentDictionary<string, AircraftTrack> tracks)
        {
            _tracks = tracks;
            _host = host;
            _port = port;

            _timeoutSec = 10; // Seconds of inactivity before removing aircraft track from dictionary
        }

        public async Task ListenForAircraftAsync()
        {
            var client = new SbsTcpClient();

            //await client.ConnectAsync("192.168.1.92", 30003, line =>
            await client.ConnectAsync(_host, _port, line =>
            {
                var msg = SbsParser.Parse(line);
                if (msg?.Latitude != null)
                {
                    Console.WriteLine($"AirCraftListener: '{msg.Icao}' '{msg.Callsign}' {msg.Latitude},{msg.Longitude} ALT {msg.Altitude}");
                }

                if (msg?.Icao != null && msg?.Latitude != null && msg.Longitude != null && msg.Altitude != null)
                {
                    _tracks.AddOrUpdate(msg.Icao,
                        // addValueFactory: Oprettes kun hvis ICAO ikke findes
                        key =>
                        {
                            var track = new AircraftTrack
                            {
                                Icao = msg.Icao,
                                Callsign = msg.Callsign,
                                LastSeen = msg.Timestamp.Value
                            };
                            track.History.Add(new PositionPoint
                            {
                                Latitude = msg.Latitude.Value,
                                Longitude = msg.Longitude.Value,
                                Altitude = msg.Altitude.Value,  //Feet
                                Timestamp = msg.Timestamp.Value
                            });
                            return track;
                        },
                        // updateValueFactory: Opdater eksisterende track
                        (key, existing) =>
                        {
                            existing.LastSeen = msg.Timestamp.Value;
                            existing.History.Add(new PositionPoint
                            {
                                Latitude = msg.Latitude.Value,
                                Longitude = msg.Longitude.Value,
                                Altitude = msg.Altitude.Value,  //Feet
                                Timestamp = msg.Timestamp.Value
                            });
                            return existing;
                        });
                }
            });
        }

        public Task RemoveExpiredTracks()
        {
            while (true)
            {
                Thread.Sleep(1000);
                lock (_tracks)
                {
                    //-- Remove track after timeout period without signal
                    List<string> toBeRemoved = new List<string>();
                    foreach (var track in _tracks)
                    {
                        TimeSpan dt = DateTime.Now - track.Value.LastSeen;
                        if (dt.TotalSeconds > _timeoutSec)
                            toBeRemoved.Add(track.Key);
                    }
                    foreach (string trackKey in toBeRemoved)
                    {
                        if (_tracks.TryRemove(trackKey, out var removed))
                        {
                            Console.WriteLine($"Track expired: {removed.Icao} (inactive for {_timeoutSec} sec)");
                        }
                    }
                }
            }
        }
    }
}

