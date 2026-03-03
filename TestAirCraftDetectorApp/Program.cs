using AirCraftDetector;
using System.Reflection.Metadata;
using System.Threading.Tasks;

Console.WriteLine("Hello, TestAirCraftDetectorApp!");

SimpleTestCases.TestForecastPositionPoint();

//Task task = SimpleTestCases.TestSbsTcpClientAsync();
//task.Wait();


//Task task = SimpleTestCases.TestSbsParserAsync();
//task.Wait();

Task task = SimpleTestCases.TestNearestAirCraftDetectorserAsync();
task.Wait();


static class SimpleTestCases
{
    public static async Task TestSbsTcpClientAsync()
    {
        var client = new SbsTcpClient();

        await client.ConnectAsync("192.168.1.92", 30003, line =>
        //await client.ConnectAsync("localhost", 30003, line =>
        {
            Console.WriteLine($"line= {line}");
        });
    }

    public static async Task TestSbsParserAsync()
    {
        var client = new SbsTcpClient();

        //await client.ConnectAsync("192.168.1.92", 30003, line =>
        await client.ConnectAsync("localhost", 30003, line =>
        {
            var msg = SbsParser.Parse(line);
            if (msg?.Latitude != null)
            {
                Console.WriteLine($"{msg.Icao} {msg.Callsign} {msg.Latitude},{msg.Longitude} ALT {msg.Altitude}");
            }
        });
    }

    public static async Task TestNearestAirCraftDetectorserAsync()
    {
        //string host = "localhost";
        string host = "192.168.1.92";
        int port = 30003;

        PositionPoint referencePoint = new PositionPoint  
        {
            Latitude = 55.860084097583304,
            Longitude = 12.4597123325542,
            Altitude = 50
        };

        var nearestAirCraftDetector = new NearestAirCraftDetector(host, port, referencePoint);

        Task task = nearestAirCraftDetector.StartTrackingAsync();
        TimeSpan durationLoop = TimeSpan.FromSeconds(4); //TODO: make this configurable

        while (true)
        {
            Thread.Sleep(4000);
            DateTime tnew = DateTime.Now + durationLoop;
            NearestAirCraftDetectorResult? result = nearestAirCraftDetector.GetNearestAirCraftAndForecastPosition(tnew);

            if (result != null)
            {
                Console.WriteLine($"AirCraftDetectorApp: Icao = '{result.AircraftTrack.Icao}', Callsign = '{result.AircraftTrack.Callsign}' {result.AircraftTrack.History.Last<PositionPoint>().ToString()}   {result.AircraftAzElForecastPosition.ToString()}");
            }
            else
            {
                Console.WriteLine("AirCraftDetectorApp: No aircraft detected.");
            }
        }
    }

    public static void TestForecastPositionPoint()
    {
        PositionPoint referencePoint = new PositionPoint
        {
            Latitude = 55,
            Longitude = 12,
            Altitude = 50
        };

        DateTime now = DateTime.Now;
        var nearestAirCraftDetector = new NearestAirCraftDetector(String.Empty, 30003, referencePoint);

        AircraftTrack track1 = new AircraftTrack();
        track1.Icao = "123456";
        track1.History.Add(new PositionPoint
        {
            Timestamp = now,
            Latitude = 55,
            Longitude = 12,
            Altitude = 500
        });

        nearestAirCraftDetector.SetFakeTracksForTesting(new List<AircraftTrack>
        {
            track1
        });
        NearestAirCraftDetectorResult? result1 = nearestAirCraftDetector.GetNearestAirCraftAndForecastPosition(now + TimeSpan.FromSeconds(20));

        string x1 = result1?.AircraftForecastPosition.ToString() ?? "null";


        track1.History.Add(new PositionPoint
        {
            Timestamp = now + TimeSpan.FromSeconds(10),
            Latitude = 55.01,
            Longitude = 12,
            Altitude = 500
        });
        NearestAirCraftDetectorResult? result2 = nearestAirCraftDetector.GetNearestAirCraftAndForecastPosition(now + TimeSpan.FromSeconds(20));
        string x2 = result2?.AircraftForecastPosition.ToString() ?? "null";


        track1.History.Add(new PositionPoint
        {
            Timestamp = now + TimeSpan.FromSeconds(20),
            Latitude = 55.04,
            Longitude = 12,
            Altitude = 500
        });
        NearestAirCraftDetectorResult? result3 = nearestAirCraftDetector.GetNearestAirCraftAndForecastPosition(now + TimeSpan.FromSeconds(30));
        string x3 = result3?.AircraftForecastPosition.ToString() ?? "null";


        track1.History.Add(new PositionPoint
        {
            Timestamp = now + TimeSpan.FromSeconds(30),
            Latitude = 55.09,
            Longitude = 12,
            Altitude = 500
        });
        NearestAirCraftDetectorResult? result4 = nearestAirCraftDetector.GetNearestAirCraftAndForecastPosition(now + TimeSpan.FromSeconds(40));
        string x4 = result4?.AircraftForecastPosition.ToString() ?? "null";


        track1.History.Add(new PositionPoint
        {
            Timestamp = now + TimeSpan.FromSeconds(30),  // <-- same timestamp as previous point, but different position
            Latitude = 55.095,
            Longitude = 12,
            Altitude = 500
        });
        NearestAirCraftDetectorResult? result5 = nearestAirCraftDetector.GetNearestAirCraftAndForecastPosition(now + TimeSpan.FromSeconds(40));
        string x5 = result5?.AircraftForecastPosition.ToString() ?? "null";


    }

}