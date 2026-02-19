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

        private readonly int _redPin = 16;
        private readonly int _yellowPin = 20;
        private readonly int _greenPin = 21;


        public FlightTrackerLEDController(HardwareMode.HardwareMode mode)
        {
            _redLedController = new LEDController(_redPin, mode); 
            _yellowLedController = new LEDController(_yellowPin, mode);
            _greenLedController = new LEDController(_greenPin, mode);
        }

        public void Initialize()
        {
                //-- Set pin mode
                _redLedController.Initialize();
                _yellowLedController.Initialize();
                _greenLedController.Initialize();

            SetFlightTrackerState(FlightTrackerState.PowerOff);
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
