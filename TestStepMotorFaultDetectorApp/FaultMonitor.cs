using System;
using System.Device.Gpio;

namespace TestStepMotorFaultDetectorApp
{
    /// <summary>
    /// Monitors a GPIO pin for a DRV8825 fault and raises an event when a fault is detected.
    /// </summary>
    public class FaultMonitor : IDisposable
    {
        private readonly GpioController _gpio;
        private const int FaultPin = 12;

        /// <summary>
        /// Raised when a fault is detected on the driver fault pin.
        /// </summary>
        public event EventHandler<FaultEventArgs>? FaultDetected;

        public FaultMonitor()
        {
            _gpio = new GpioController();

            _gpio.OpenPin(FaultPin, PinMode.Input);

            // Register internal callback which will raise the public event
            _gpio.RegisterCallbackForPinValueChangedEvent(
                FaultPin,
                PinEventTypes.Falling,
                OnPinFaultDetected);
        }

        private void OnPinFaultDetected(object? sender, PinValueChangedEventArgs e)
        {
            var args = new FaultEventArgs(DateTime.Now);

            // Raise event for subscribers (Program.cs will handle logging/response)
            FaultDetected?.Invoke(this, args);
        }

        /// <summary>
        /// Returns true if the fault pin reads low (active low).
        /// </summary>
        public bool IsFault()
        {
            return _gpio.Read(FaultPin) == PinValue.Low;
        }

        public void Dispose()
        {
            _gpio?.Dispose();
        }
    }
}
