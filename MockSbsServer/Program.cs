using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;

Console.WriteLine("MockSbsServer - Simulating ADS-B SBS-1 data stream");
Console.WriteLine("Listening on port 30003...\n");

var server = new MockSbsServer();
await server.StartAsync(30003);

public class MockSbsServer
{
    private readonly List<SimulatedAircraft> _aircrafts = new();
    private readonly Random _random = new();

    public async Task StartAsync(int port)
    {
        // Initialize simulated aircraft
        InitializeAircraft();

        var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"Server started on port {port}");

        // Accept clients
        _ = Task.Run(async () =>
        {
            while (true)
            {
                var client = await listener.AcceptTcpClientAsync();
                Console.WriteLine($"Client connected: {client.Client.RemoteEndPoint}");
                _ = HandleClientAsync(client);
            }
        });

        // Update aircraft positions
        while (true)
        {
            UpdateAircraft();
            await Task.Delay(1000);
        }
    }

    private void InitializeAircraft()
    {
        // Copenhagen coordinates: 55.6761° N, 12.5683° E
        double baseLat = 55.6761;
        double baseLon = 12.5683;

        _aircrafts.Add(new SimulatedAircraft
        {
            Icao = "4CA1D2",
            Callsign = "SAS123",
            Latitude = baseLat + 0.1,
            Longitude = baseLon + 0.1,
            Altitude = 12000,  // Feet
            GroundSpeed = 450,
            Track = 90,
            VerticalRate = 500
        });

        _aircrafts.Add(new SimulatedAircraft
        {
            Icao = "471F5D",
            Callsign = "NAX456",
            Latitude = baseLat - 0.15,
            Longitude = baseLon + 0.2,
            Altitude = 18000,  // Feet
            GroundSpeed = 420,
            Track = 270,
            VerticalRate = -200
        });

        _aircrafts.Add(new SimulatedAircraft
        {
            Icao = "4CA8E7",
            Callsign = "DLH789",
            Latitude = baseLat + 0.2,
            Longitude = baseLon - 0.1,
            Altitude = 25000,  // Feet
            GroundSpeed = 480 * 10,   //TODO: FAST AIRCRAFT :-)
            Track = 180,
            VerticalRate = 0
        });
    }

    private void UpdateAircraft()
    {
        foreach (var ac in _aircrafts)
        {
            // Update position based on speed and track
            double speedKmPerSec = ac.GroundSpeed / 3600.0;
            double latChange = speedKmPerSec * Math.Cos(ac.Track * Math.PI / 180.0) / 111.0;
            double lonChange = speedKmPerSec * Math.Sin(ac.Track * Math.PI / 180.0) / (111.0 * Math.Cos(ac.Latitude * Math.PI / 180.0));

            ac.Latitude += latChange;
            ac.Longitude += lonChange;

            bool keepElevationAndSpeed = true;
            if (!keepElevationAndSpeed)
            {
                ac.Altitude += ac.VerticalRate / 60;  // feet per second   //TODO: Require 1 update pr miinute ? ? 

                // Add some random variation
                ac.Track += _random.NextDouble() * 2 - 1;
                ac.GroundSpeed += _random.NextDouble() * 10 - 5;
                ac.VerticalRate += _random.Next(-50, 50);

                // Keep values in reasonable ranges
                ac.GroundSpeed = Math.Max(200, Math.Min(600, ac.GroundSpeed));
                ac.Altitude = Math.Max(1000, Math.Min(40000, ac.Altitude));
                ac.Track = (ac.Track + 360) % 360;
            }
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        try
        {
            await using var stream = client.GetStream();
            using var writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };

            while (client.Connected)
            {
                foreach (var ac in _aircrafts)
                {
                    // Generate SBS-1 format message
                    string sbsMessage = GenerateSbsMessage(ac);
                    await writer.WriteLineAsync(sbsMessage);
                    Console.WriteLine(sbsMessage);
                }

                await Task.Delay(1000);  // Send updates every second
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Client disconnected: {ex.Message}");
        }
    }

    private string GenerateSbsMessage(SimulatedAircraft ac)
    {
        DateTime now = DateTime.UtcNow;
        string dateStr = now.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
        string timeStr = now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture);

        //                        4      6   7    8    9    10       11         12        13    14  15   16          17     18    19        20  21
        // SBS-1 format: MSG,3,,,ICAO,,date,time,date,time,callsign,altitude,groundspeed,track,lat,lon,verticalrate,squawk,alert,emergency,spi,onground

        return string.Create(CultureInfo.InvariantCulture, 
               $"MSG,3,1,1,{ac.Icao},1,{dateStr},{timeStr},{dateStr},{timeStr}," +
               $"{ac.Callsign},{ac.Altitude:F0},{ac.GroundSpeed:F0},{ac.Track:F1}," +
               $"{ac.Latitude:F6},{ac.Longitude:F6},{ac.VerticalRate:F0},,,,0,0");
    }
}

public class SimulatedAircraft
{
    public string Icao { get; set; }
    public string Callsign { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Altitude { get; set; }
    public double GroundSpeed { get; set; }
    public double Track { get; set; }
    public int VerticalRate { get; set; }
}