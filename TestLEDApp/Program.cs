using LEDController;

Console.WriteLine("Red LED controlled by LEDControllers");

int ledPin = 17; // GPIO pin number for the LED

LedHardwareMode mode= LedHardwareMode.Emulated; // Change to Real for actual hardware

using (LEDController.LEDController led = new LEDController.LEDController(ledPin, mode))
{
    led.Initialize();

    led.SetState(LEDController.LEDController.LEDState.On);

    led.SetState(LEDController.LEDController.LEDState.Blinking);
    Thread.Sleep(10000);

    led.SetState(LEDController.LEDController.LEDState.Off);
}


Console.WriteLine("LED controlled by FlightTrackerLEDController ");

using (FlightTrackerLEDController flightTrackerLEDController = new FlightTrackerLEDController(mode))
{
    Console.WriteLine($"State = {FlightTrackerState.NoAirCraftFound}");
    flightTrackerLEDController.SetFlightTrackerState(FlightTrackerState.NoAirCraftFound);
    Thread.Sleep(2000);

    Console.WriteLine($"State = {FlightTrackerState.MovingToAirCraft}");
    flightTrackerLEDController.SetFlightTrackerState(FlightTrackerState.MovingToAirCraft);
    Thread.Sleep(5000);

    Console.WriteLine($"State = {FlightTrackerState.TrackingAirCraft}");
    flightTrackerLEDController.SetFlightTrackerState(FlightTrackerState.TrackingAirCraft);
    Thread.Sleep(2000);

    Console.WriteLine($"State = {FlightTrackerState.PowerOff}");
    flightTrackerLEDController.SetFlightTrackerState(FlightTrackerState.PowerOff);
    Thread.Sleep(2000);
}


