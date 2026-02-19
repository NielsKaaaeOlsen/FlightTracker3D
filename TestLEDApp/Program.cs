using LEDController;

Console.WriteLine("Red LED controlled by LEDController");

int ledPin = 16; // GPIO pin number for the LED

HardwareMode.HardwareMode mode= HardwareMode.HardwareMode.Real; // Change to Real for actual hardware

using (LEDController.LEDController led = new LEDController.LEDController(ledPin, mode))
{
    led.Initialize();

    led.SetState(LEDController.LEDController.LEDState.On);

    led.SetState(LEDController.LEDController.LEDState.Blinking);
    Thread.Sleep(10000);

    led.SetState(LEDController.LEDController.LEDState.Off);
}

Console.WriteLine("");
Console.WriteLine("");
Console.WriteLine("Red, Yellow and Green LED's controlled by FlightTrackerLEDController ");

using (FlightTrackerLEDController flightTrackerLEDController = new FlightTrackerLEDController(mode))
{
    flightTrackerLEDController.Initialize();

    Console.WriteLine($"State = {FlightTrackerState.NoAirCraftFound}    {DateTime.Now.ToString()}");
    flightTrackerLEDController.SetFlightTrackerState(FlightTrackerState.NoAirCraftFound);
    Thread.Sleep(5000);

    Console.WriteLine($"State = {FlightTrackerState.MovingToAirCraft}   {DateTime.Now.ToString()}");
    flightTrackerLEDController.SetFlightTrackerState(FlightTrackerState.MovingToAirCraft);
    Thread.Sleep(5000);

    Console.WriteLine($"State = {FlightTrackerState.TrackingAirCraft}   {DateTime.Now.ToString()}");
    flightTrackerLEDController.SetFlightTrackerState(FlightTrackerState.TrackingAirCraft);
    Thread.Sleep(5000);

    Console.WriteLine($"State = {FlightTrackerState.PowerOff}           {DateTime.Now.ToString()}");
    flightTrackerLEDController.SetFlightTrackerState(FlightTrackerState.PowerOff);
    Thread.Sleep(5000);
}


