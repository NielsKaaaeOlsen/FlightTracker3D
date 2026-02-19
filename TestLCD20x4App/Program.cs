using HardwareMode;
using LCDController;

Console.WriteLine("Testing FlightTracker LCD 20x4C ontroller ");

HardwareModeEnum mode = HardwareModeEnum.Real; // Change to Real for actual hardware

using (TrackInfoPresenter lcdPresenter = new TrackInfoPresenter(mode))
{
    lcdPresenter.Initialize();

    Console.WriteLine("\nApp->NoTracks");
    lcdPresenter.NoTracks();
    Thread.Sleep(5000); // Keep the display on for a while to see the output

    Console.WriteLine("\nApp->MovingToNewPosition");
    lcdPresenter.ApproachingTarget(11.1, 22.2);
    Thread.Sleep(5000); // Keep the display on for a while to see the output

    Console.WriteLine("App->Tracking  #1");
    lcdPresenter.AircraftTracking(az: 123.4, el: 123.4, altMeter: 12345.1, dist: 12345.1, callsign: "SAS1088", icao: "0x4456");
    Thread.Sleep(5000); // Keep the display on for a while to see the output

    Console.WriteLine("App->Tracking #2");
    lcdPresenter.AircraftTracking(az: 223.4, el: 223.4, altMeter: 22345.1, dist: 22345.1, callsign: "SAS1088", icao: "0x4456");
    Thread.Sleep(5000); // Keep the display on for a while to see the output


    Console.WriteLine("App->Tracking #3");
    lcdPresenter.AircraftTracking(az: 323.4, el: 323.4, altMeter: 32345.1, dist: 32345.1, callsign: "SAS1088", icao: "0x4456");


    Thread.Sleep(5000); // Keep the display on for a while to see the output

    Console.WriteLine("App->Blank");
    lcdPresenter.Blank();
}

