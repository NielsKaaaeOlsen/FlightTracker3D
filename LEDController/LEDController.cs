using System.Device.Gpio;

namespace LEDController
{
    public enum LedHardwareMode
    {
        Real,
        Emulated
    }
    public class LEDController : IDisposable
    {
        private LEDState _currentState;

        private GpioController _gpioController;
        private LedHardwareMode _hardwareMode;
        private bool _isBlinkingOn;
        private int _ledPin;

        public enum LEDState
        {
            Off,
            Blinking,
            On
        }
        public LEDController(int ledPin, LedHardwareMode mode)
        {
            _ledPin = ledPin;
            _hardwareMode = mode;
            _currentState = LEDState.Off;

            if (_hardwareMode == LedHardwareMode.Real)
            {
                _gpioController = new GpioController();
            }


        }

        public void Dispose()
        {
            if (_hardwareMode == LedHardwareMode.Real)
            {
                ((IDisposable)_gpioController).Dispose();
            }
        }

        public void Initialize()
        {
            if (_hardwareMode == LedHardwareMode.Real)
            {
                //-- Set pin mode
                _gpioController.OpenPin(_ledPin, PinMode.Output);
            }
            SetState(_currentState);
        }
        public void SetState(LEDState state)
        {
            _currentState = state;
            Console.WriteLine($"LED state set to: {_currentState}");

            switch (_currentState)
            {
                case LEDState.Off:
                    if (_hardwareMode == LedHardwareMode.Real)
                        _gpioController.Write(_ledPin, PinValue.Low);
                    else
                        Console.WriteLine("LED is off (emulated mode).");
                    break;
                case LEDState.On:
                    if (_hardwareMode == LedHardwareMode.Real)
                        _gpioController.Write(_ledPin, PinValue.High);
                    else
                        Console.WriteLine("LED is on (emulated mode).");
                    break;
                case LEDState.Blinking:
                    _isBlinkingOn = true;
                    Task task = Task.Run(() =>
                    {
                        while (_currentState == LEDState.Blinking)
                        {
                            Console.WriteLine($"LED is blinking... {_isBlinkingOn}");
                            if (_hardwareMode == LedHardwareMode.Real)
                                _gpioController.Write(_ledPin, _isBlinkingOn ? PinValue.High : PinValue.Low);
                            else
                                Console.WriteLine($"LED is {(_isBlinkingOn ? "On": "off ")}(emulated mode).");

                            Thread.Sleep(1000); // Simulate blinking every second
                            _isBlinkingOn = !_isBlinkingOn; // Toggle the blinking state
                        }
                    });
                    break;
                default:
                    break;
            }
        }
    }
}
