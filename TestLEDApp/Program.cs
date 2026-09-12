using HardwareMode;
using LEDController;

HardwareModeEnum mode= HardwareModeEnum.Real; // Change to Real for actual hardware

Console.WriteLine("Testing Red, yellow and green led's by LEDController");
int[] ledPins = new int[] { 16, 20, 21 }; // GPIO pin numbers for the red, yellow and green LED's 
for (int i = 0; i < ledPins.Length; i++)
{
    Console.WriteLine($"Pin {ledPins[i]} is {(HardwareModeEnum.Real == mode ? "Real" : "Emulated")}");

    using (LEDController.LEDController led = new LEDController.LEDController(ledPins[i], mode, ledPins[i].ToString()))
    {
        led.Initialize();

        led.SetState(LEDController.LEDController.LEDState.On);
        Thread.Sleep(2500);

        led.SetState(LEDController.LEDController.LEDState.Blinking);
        Thread.Sleep(10000);

        led.SetState(LEDController.LEDController.LEDState.Off);
        Thread.Sleep(2500);
    }
}

Console.WriteLine("");
Console.WriteLine("");
Console.WriteLine("Testing Red, Yellow and Green LED's controlled by FlightTrackerLEDController ");

using (FlightTrackerLEDController flightTrackerLEDController = new FlightTrackerLEDController(mode))
{
    flightTrackerLEDController.Initialize();

    Console.WriteLine($"State = {FlightTrackerState.NoAirCraftFound}    {DateTime.UtcNow.ToString()}");
    flightTrackerLEDController.SetFlightTrackerState(FlightTrackerState.NoAirCraftFound);
    Thread.Sleep(5000);

    Console.WriteLine($"State = {FlightTrackerState.MovingToAirCraft}   {DateTime.UtcNow.ToString()}");
    flightTrackerLEDController.SetFlightTrackerState(FlightTrackerState.MovingToAirCraft);
    Thread.Sleep(5000);

    Console.WriteLine($"State = {FlightTrackerState.TrackingAirCraft}   {DateTime.UtcNow.ToString()}");
    flightTrackerLEDController.SetFlightTrackerState(FlightTrackerState.TrackingAirCraft);
    Thread.Sleep(5000);

    Console.WriteLine($"State = {FlightTrackerState.PowerOff}           {DateTime.UtcNow.ToString()}");
    flightTrackerLEDController.SetFlightTrackerState(FlightTrackerState.PowerOff);
    Thread.Sleep(5000);
}


