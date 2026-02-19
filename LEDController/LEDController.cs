using System.Device.Gpio;
using HardwareMode;

namespace LEDController
{
    /// <summary>
    /// Controls a single LED connected to a GPIO pin with support for on/off/blinking states.
    /// Supports both real hardware and emulated mode for development/testing.
    /// </summary>
    public class LEDController : IDisposable
    {
        /// <summary>
        /// Defines the possible states of an LED.
        /// </summary>
        public enum LEDState
        {
            /// <summary>
            /// LED is turned off.
            /// </summary>
            Off,
            
            /// <summary>
            /// LED is blinking on and off at 1 Hz (1 second intervals).
            /// </summary>
            Blinking,
            
            /// <summary>
            /// LED is constantly on.
            /// </summary>
            On
        }

        private LEDState _currentState;
        private GpioController _gpioController;
        private HardwareModeEnum _hardwareMode;
        private bool _isBlinkingOn;
        private int _ledPin;

        /// <summary>
        /// Initializes a new instance of the <see cref="LEDController"/> class.
        /// </summary>
        /// <param name="ledPin">GPIO pin number where the LED is connected.</param>
        /// <param name="mode">Hardware mode (Real or Emulated).</param>
        public LEDController(int ledPin, HardwareModeEnum mode)
        {
            _ledPin = ledPin;
            _hardwareMode = mode;
            _currentState = LEDState.Off;

            if (_hardwareMode == HardwareModeEnum.Real)
            {
                _gpioController = new GpioController();
            }
        }

        /// <summary>
        /// Initializes the LED controller and configures the GPIO pin as output.
        /// Must be called before using <see cref="SetState(LEDState)"/>.
        /// </summary>
        public void Initialize()
        {
            if (_hardwareMode == HardwareModeEnum.Real)
            {
                _gpioController.OpenPin(_ledPin, PinMode.Output);
            }
            SetState(_currentState);
        }

        /// <summary>
        /// Sets the LED state (Off, On, or Blinking).
        /// </summary>
        /// <param name="state">The desired LED state.</param>
        /// <remarks>
        /// In Blinking mode, the LED toggles every second on a background thread.
        /// Call <see cref="SetState(LEDState)"/> with <see cref="LEDState.Off"/> or 
        /// <see cref="LEDState.On"/> to stop blinking.
        /// </remarks>
        public void SetState(LEDState state)
        {
            _currentState = state;
            Console.WriteLine($"LED state set to: {_currentState}");

            switch (_currentState)
            {
                case LEDState.Off:
                    if (_hardwareMode == HardwareModeEnum.Real)
                        _gpioController.Write(_ledPin, PinValue.Low);
                    else
                        Console.WriteLine("LED is off (emulated mode).");
                    break;

                case LEDState.On:
                    if (_hardwareMode == HardwareModeEnum.Real)
                        _gpioController.Write(_ledPin, PinValue.High);
                    else
                        Console.WriteLine("LED is on (emulated mode).");
                    break;

                case LEDState.Blinking:
                    _isBlinkingOn = true;
                    Task.Run(() =>
                    {
                        while (_currentState == LEDState.Blinking)
                        {
                            Console.WriteLine($"LED is blinking... {_isBlinkingOn}");
                            if (_hardwareMode == HardwareModeEnum.Real)
                                _gpioController.Write(_ledPin, _isBlinkingOn ? PinValue.High : PinValue.Low);
                            else
                                Console.WriteLine($"LED is {(_isBlinkingOn ? "On" : "off")} (emulated mode).");

                            Thread.Sleep(1000);
                            _isBlinkingOn = !_isBlinkingOn;
                        }
                    });
                    break;
            }
        }

        /// <summary>
        /// Releases all resources used by the <see cref="LEDController"/>.
        /// Disposes the GPIO controller if in Real mode.
        /// </summary>
        public void Dispose()
        {
            if (_hardwareMode == HardwareModeEnum.Real)
            {
                _gpioController?.Dispose();
            }
        }
    }
}
