using LCDController;

Console.WriteLine("Testing FlightTracker LCD 20x4C ontroller ");

LcdHardwareMode mode = LcdHardwareMode.Emulated; // Change to Real for actual hardware

using (FlightTRackerLCD20x4Controller lcd20X4 = new FlightTRackerLCD20x4Controller(mode))
{
    lcd20X4.Initialize();

    Console.WriteLine("\nNoTracks");
    lcd20X4.NoTracks();

    Console.WriteLine("\nMovingToNewPosition");
    lcd20X4.ApproachingTarget(11.1, 22.2);

    Console.WriteLine("Tracking");
    lcd20X4.Repositioning(az: 123.4, el: 123.4, altMeter: 12345.1, dist: 12345.1, callsign: "SAS1088", icao: "0x4456");



}

