using AirCraftDetector;
using System.Reflection.Metadata;
using System.Threading.Tasks;

Console.WriteLine("Hello, TestAirCraftDetectorApp!");


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

        //await client.ConnectAsync("192.168.1.92", 30003, line =>
        await client.ConnectAsync("localhost", 30003, line =>
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
        string host = "localhost";
        int port = 30003;

        PositionPoint referencePoint = new PositionPoint  
        {
            Latitude = 55.860084097583304,
            Longitude = 12.4597123325542,
            Altitude = 50
        };

        var nearestAirCraftDetector = new NearestAirCraftDetector(host, port, referencePoint);

        Task task = nearestAirCraftDetector.Initialize();

        while (true)
        {
            Thread.Sleep(4000);
            var nearestAirCraft = nearestAirCraftDetector.GetNearestAirCraft();

            if (nearestAirCraft?.AircraftTrack != null)
            {
                Console.WriteLine($"nearestAirCraft: Icao = {nearestAirCraft.AircraftTrack.Icao}");
            }
            else
            {
                Console.WriteLine("No aircraft detected.");
            }
        }
    }

}