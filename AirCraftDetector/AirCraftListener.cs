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
        public AirCraftListener(string host, int port, ConcurrentDictionary<string, AircraftTrack> tracks) 
        { 
            _tracks = tracks;
            _host = host;
            _port = port;
        }

        public async void Listen()
        {
            var client = new SbsTcpClient();

            //await client.ConnectAsync("192.168.1.92", 30003, line =>
            await client.ConnectAsync(_host, _port, line =>
            {
                var msg = SbsParser.Parse(line);
                if (msg?.Latitude != null)
                {
                    Console.WriteLine($"AirCraftListener: {msg.Icao} {msg.Callsign} {msg.Latitude},{msg.Longitude} ALT {msg.Altitude}");
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
    }
}
