using Microsoft.Extensions.Logging;
using System.Device.Gpio;

using System.Diagnostics;
using System.Threading;
using HardwareMode;

namespace StepperMotorController
{
    public class StepperMotorController : IDisposable
    {
        private readonly StepperMotorPins _pins;
        private MicrosteppingMode _microSteppingMode;
        private readonly GpioController _gpioController;

        private int _microStepsPerStep;
        private double _degreesPerStep;
        private readonly ILogger _logger;
        private HardwareModeEnum _hardwareMode;

        public StepperMotorController(ILoggerFactory loggerFactory, StepperMotorPins pins, string instanceId, HardwareModeEnum mode)
        {
            _logger = loggerFactory.CreateLogger($"{typeof(StepperMotorController).Name}:{instanceId}");

            _hardwareMode = mode;
            _pins = pins;
            _microSteppingMode = MicrosteppingMode.M8;  // Default to 1/8 microstepping for MoveTo speed

            if (_hardwareMode == HardwareModeEnum.Real)
            {
                _gpioController = new GpioController();
            }
        }

        public void Initialize()
        {
            // Initialize GPIO pins for stepper motor control
            _logger.LogInformation(
                "Initializing Stepper Motor on Pins: Step={StepPin}, Dir={DirPin}, M0={M0Pin}, M1={M1Pin}, M2={M2Pin}",
                _pins.StepPin, _pins.DirPin, _pins.M0Pin, _pins.M1Pin, _pins.M2Pin);

            if (_hardwareMode == HardwareModeEnum.Real)
            {
                //-- Set pin mode
                _gpioController.OpenPin(_pins.StepPin, PinMode.Output);
                _gpioController.OpenPin(_pins.DirPin, PinMode.Output);
                _gpioController.OpenPin(_pins.M0Pin, PinMode.Output);
                _gpioController.OpenPin(_pins.M1Pin, PinMode.Output);
                _gpioController.OpenPin(_pins.M2Pin, PinMode.Output);

                //SetMicrostepping(MicrosteppingMode.M32);  // Use 1/32 microstepping for timed steps

                //// Sæt 1/32 microstepping (M2 = H, M1 = L, M0 = H) 
                //_gpioController.Write(_pins.M0Pin, PinValue.High);
                //_gpioController.Write(_pins.M1Pin, PinValue.Low);
                //_gpioController.Write(_pins.M2Pin, PinValue.High);
            }
            //_microStepsPerStep = 2;  // 1 full step = 32 microsteps (1/32 microstepping)
            int stepsPerRevolution = 200;  // Assuming 200 full steps per revolution
            _degreesPerStep = 360.0 / stepsPerRevolution;
        }

        public double GetDegreesPerStep()
        {
            return _degreesPerStep;
        }


        /*
        M0	M1	M2	Microstep Resolution
        L	L	L	Full step (1)
        H	L	L	1/2 step
        L	H	L	1/4 step
        H	H	L	1/8 step
        L	L	H	1/16 step
        H	L	H	1/32 step
        L	H	H	1/32 step
        H	H	H	1/32 step
        */


        public void SetMicrostepping(MicrosteppingMode mode)
        {
            _microSteppingMode = mode;
        }

        public void SetMicrosteppingPins(MicrosteppingMode mode )
        {
            // Set microstepping mode based on desired microsteps per step
            switch(mode)
            {
                case MicrosteppingMode.M32:
                    if (_hardwareMode == HardwareModeEnum.Real)
                    {
                        _gpioController.Write(_pins.M0Pin, PinValue.High);
                        _gpioController.Write(_pins.M1Pin, PinValue.Low);
                        _gpioController.Write(_pins.M2Pin, PinValue.High);
                    }
                    _microStepsPerStep = 32;
                    break;

                case MicrosteppingMode.M8:
                    if (_hardwareMode == HardwareModeEnum.Real)
                    {
                        _gpioController.Write(_pins.M0Pin, PinValue.High);
                        _gpioController.Write(_pins.M1Pin, PinValue.High);
                        _gpioController.Write(_pins.M2Pin, PinValue.Low);
                    }
                    _microStepsPerStep = 8;
                    break;

                case MicrosteppingMode.M4:
                    if (_hardwareMode == HardwareModeEnum.Real)
                    {
                        _gpioController.Write(_pins.M0Pin, PinValue.Low);
                        _gpioController.Write(_pins.M1Pin, PinValue.High);
                        _gpioController.Write(_pins.M2Pin, PinValue.Low);
                    }
                    _microStepsPerStep = 4;
                    break;

                case MicrosteppingMode.M2:
                    if (_hardwareMode == HardwareModeEnum.Real)
                    {
                        _gpioController.Write(_pins.M0Pin, PinValue.High);  
                        _gpioController.Write(_pins.M1Pin, PinValue.Low);
                        _gpioController.Write(_pins.M2Pin, PinValue.Low);
                    }
                    _microStepsPerStep = 2;
                    break;

                default:
                    throw new ArgumentException($"Unsupported microstepping mode: {mode}");
            }
        }

        public void Step(bool forward, int steps, double timePerStepSec)
        {
            if (timePerStepSec != 0)
                SetMicrosteppingPins(_microSteppingMode);  
            else
            {
                SetMicrosteppingPins(MicrosteppingMode.M2);   // Use 1/2 microstepping for maximum speed
                // For maximum speed, set timePerStepSec to a very small value. We will use a value similar to 10 sec for a full revolution
                timePerStepSec = 10.0 / 200.0;  // 10 sec per revolution / 200 steps per revolution = 0.05 sec per step
            }

            DateTime startTime = DateTime.Now;

            _logger.LogInformation($"Step: forward={forward}, steps={steps}, timePerStepSec={timePerStepSec}");

            int timePerStepMilliSec = (int)(timePerStepSec * 1000);
            int dtmsPerMicroStep = timePerStepMilliSec / _microStepsPerStep;

            int delayHigh = dtmsPerMicroStep / 2;
            int delayLow = dtmsPerMicroStep / 2;
            if (dtmsPerMicroStep - delayHigh - delayLow > 0) 
                delayLow++;

            //-- Set direction pin values  -->  Retning: High = CW, Low = CCW (afhænger af kabler) 
            PinValue dirValue = forward ? PinValue.High : PinValue.Low;
            if (_hardwareMode == HardwareModeEnum.Real)
                _gpioController.Write(_pins.DirPin, dirValue);

            //-- Set microsteppings pin values
            for (int iStep = 0; iStep < steps; iStep++)
            {
                _logger.LogInformation("iStep={iStep}", iStep);
                for (int iMicroStep = 0; iMicroStep < _microStepsPerStep; iMicroStep++)
                {
                    _logger.LogDebug("iStep={iStep}, iMicroStep={iMicroStep}, pinVal=HIGH", iStep, iMicroStep);
                    // Set the step pin high
                    if (_hardwareMode == HardwareModeEnum.Real)
                        _gpioController.Write(_pins.StepPin, PinValue.High);
                    DelayMilliSeconds(delayHigh);
                    // Set the step pin low
                    _logger.LogDebug("iStep={iStep}, iMicroStep={iMicroStep}, pinVal=LOW", iStep, iMicroStep);
                    if (_hardwareMode == HardwareModeEnum.Real)
                        _gpioController.Write(_pins.StepPin, PinValue.Low);
                    DelayMilliSeconds(delayLow);
                }
            }

            DateTime endTime = DateTime.Now;
            TimeSpan duration = endTime - startTime;
            _logger.LogInformation("-->Stepping completed in {duration} ms,   Expected {exp} ms or {expSec} sec", 
                duration.TotalMilliseconds, steps * (delayHigh + delayLow) * _microStepsPerStep, steps * timePerStepSec);
        }

        // lightweight async wrapper — runs the existing synchronous Step on the threadpool
        public Task StepAsync(bool forward, int steps, double timePerStepSec)
        {
            // preserve current behavior, run on ThreadPool so caller can await
            return Task.Run(() => Step(forward, steps, timePerStepSec));
        }

        public int GetMicroStepsPerStep()
        {
            return _microStepsPerStep;
        }

        static void DelayMicroseconds_not_working(int microseconds)
        {
            long ticks = microseconds * (Stopwatch.Frequency / 1_000_000);
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedTicks < ticks)
            {
                Thread.SpinWait(10);
            }
        }

        static void DelayMilliSeconds(int milliseconds)
        {
            Thread.Sleep(milliseconds);
        }

        public void Dispose()
        {
            if (_hardwareMode == HardwareModeEnum.Real)
            {
                ((IDisposable)_gpioController).Dispose();
            }
        }
    }
}
