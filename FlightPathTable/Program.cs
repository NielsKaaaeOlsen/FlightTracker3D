

using AirCraftDetector;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

string sbsMessageFolder = $"C:\\Users\\niels\\source\\repos\\FlightTracker3D\\SbsMessageRecorderApp\\bin\\Release\\net8.0";

string sbsMessageFilePath = Path.Combine(sbsMessageFolder, "SbsMessages_20260904_100148.txt");


string tableFilePath = $"FlightPathTable_{DateTime.UtcNow:yyyyMMdd_HHmmss}.txt";

// Create a FlightPathTable instance and
// load the SBS messages from the specified file

//Load all SBS messages from the specified file
string[] sbsMessages = File.ReadAllLines(sbsMessageFilePath);

PositionPoint referencePoint = new PositionPoint
{
    Latitude = 55.860084097583304,
    Longitude = 12.4597123325542,
    Altitude = 50
};

string icaoFilter = "4CAD91";


using (StreamWriter writer = new StreamWriter(tableFilePath))
{
    foreach (string sbsMessage in sbsMessages)
    {     // Parse the SBS message and update the flight path table
        var msg = SbsParser.Parse(sbsMessage);
        if (msg != null)
        {
            if (msg.Icao == icaoFilter && msg.Latitude != null && msg.Longitude != null && msg.Altitude != null)
            {
                PositionPoint pos = new PositionPoint
                {
                    Latitude = msg.Latitude.Value,
                    Longitude = msg.Longitude.Value,
                    Altitude = msg.Altitude.Value
                };
                AzElPosition azElPos = referencePoint.ToAzElPosition(pos);
                double distance = referencePoint.DistanceTo(pos);

                Console.WriteLine($"Timestamp: {msg.Timestamp}, ICAO: {msg.Icao}, Callsign: {msg.Callsign}, Position: ({pos.Latitude}, {pos.Longitude}, {pos.Altitude}), Azimuth: {azElPos.Azimuth}, Elevation: {azElPos.Elevation}, Distance: {distance}");
                writer.WriteLine( $"Timestamp: {msg.Timestamp}, ICAO: {msg.Icao}, Callsign: {msg.Callsign}, Position: ({pos.Latitude}, {pos.Longitude}, {pos.Altitude}), Azimuth: {azElPos.Azimuth}, Elevation: {azElPos.Elevation}, Distance: {distance}");

            }
        }
    }
}
Console.WriteLine("By NKO, September 2026");