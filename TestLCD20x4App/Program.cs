using LCDController;

Console.WriteLine("Testing FlightTracker LCD 20x4C ontroller ");

LcdHardwareMode mode = LcdHardwareMode.Emulated; // Change to Real for actual hardware

using (TrackInfoPresenter lcd20X4 = new TrackInfoPresenter(mode))
{
    lcd20X4.Initialize();

    Console.WriteLine("\nApp->NoTracks");
    lcd20X4.NoTracks();
    Thread.Sleep(5000); // Keep the display on for a while to see the output

    Console.WriteLine("\nApp->MovingToNewPosition");
    lcd20X4.ApproachingTarget(11.1, 22.2);
    Thread.Sleep(5000); // Keep the display on for a while to see the output

    Console.WriteLine("App->Tracking");
    lcd20X4.AircraftTracking(az: 123.4, el: 123.4, altMeter: 12345.1, dist: 12345.1, callsign: "SAS1088", icao: "0x4456");
    Thread.Sleep(5000); // Keep the display on for a while to see the output

    Console.WriteLine("App->Tracking");
    lcd20X4.AircraftTracking(az: 223.4, el: 223.4, altMeter: 22345.1, dist: 22345.1, callsign: "SAS1088", icao: "0x4456");


    Thread.Sleep(5000); // Keep the display on for a while to see the output


}

