using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LEDController
{
    public enum FlightTrackerState
    {
        PowerOff,
        NoAirCraftFound,
        MovingToAirCraft,
        TrackingAirCraft
    }
    public class FlightTrackerLEDController : IDisposable
    {
        private LEDController _redLedController;
        private LEDController _yellowLedController;
        private LEDController _greenLedController;

        public FlightTrackerLEDController(LedHardwareMode mode)
        {
            _redLedController = new LEDController(17, mode);   //TODO: Define GPIO pins for the LEDs
            _yellowLedController = new LEDController(18, mode);
            _greenLedController = new LEDController(19, mode);
        }

        public void Dispose()
        {
            _redLedController.Dispose();
            _yellowLedController.Dispose();
            _greenLedController.Dispose();
        }


        public void SetFlightTrackerState(FlightTrackerState state)
        {
            switch (state)
            {
                case FlightTrackerState.NoAirCraftFound:
                    _redLedController.SetState(LEDController.LEDState.On);
                    _yellowLedController.SetState(LEDController.LEDState.Off);
                    _greenLedController.SetState(LEDController.LEDState.Off);
                    break;

                case FlightTrackerState.MovingToAirCraft:
                    _redLedController.SetState(LEDController.LEDState.Off);
                    _yellowLedController.SetState(LEDController.LEDState.Blinking);
                    _greenLedController.SetState(LEDController.LEDState.Off);
                    break;

                case FlightTrackerState.TrackingAirCraft:
                    _redLedController.SetState(LEDController.LEDState.Off);
                    _yellowLedController.SetState(LEDController.LEDState.Off);
                    _greenLedController.SetState(LEDController.LEDState.Blinking);
                    break;

                    case FlightTrackerState.PowerOff:
                    _redLedController.SetState(LEDController.LEDState.Off);
                    _yellowLedController.SetState(LEDController.LEDState.Off);
                    _greenLedController.SetState(LEDController.LEDState.Off);
                    break;
            }

        }
    }
}
