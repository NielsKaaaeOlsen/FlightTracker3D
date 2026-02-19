using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HardwareMode;

namespace StepperMotorController
{
    public class AzElController : IDisposable
    {


        private readonly StepperMotorController _aziumuthController;
        private readonly StepperMotorController _elevationController;

        private int _currentAzStep;
        private int _currentElStep;

        private ILogger _logger;
        private HardwareModeEnum _hardwareMode;

        public AzElController(ILoggerFactory loggerFactory, HardwareModeEnum mode)
        {
            _logger = loggerFactory.CreateLogger($"{typeof(AzElController).Name}");

            _hardwareMode = mode;

            var azPins = new StepperMotorPins(stepPin: 17, dirPin: 27, m0Pin: 22, m1Pin: 23, m2Pin: 24);
            var elPins = new StepperMotorPins(stepPin: 6, dirPin: 13, m0Pin: 22, m1Pin: 23, m2Pin: 24);

            _aziumuthController = new StepperMotorController(loggerFactory, azPins, "Aziumuth", _hardwareMode);
            _elevationController = new StepperMotorController(loggerFactory, elPins, "Elevation", _hardwareMode);
        }


        public void Initialize()
        {
            _logger.LogInformation("Initializing Azimuth and Elevation Stepper Motors");

            _aziumuthController.Initialize();
            _elevationController.Initialize();

            _currentAzStep = 0;
            _currentElStep = 0;

        }

        public void SetMicrostepping(MicrosteppingMode mode)
        {
            _aziumuthController.SetMicrostepping(mode);
            _elevationController.SetMicrostepping(mode);
        }
        public void MoveToAzEl(double azimuthDegrees, double elevationDegrees, double durationSeconds = 0)
        {
            //Validate angles
            if (azimuthDegrees < 0 || azimuthDegrees >= 360) 
                throw new ArgumentOutOfRangeException(nameof(azimuthDegrees), "Azimuth degrees must be between 0 and 360.");
            if (elevationDegrees < 0 || elevationDegrees > 90) 
                throw new ArgumentOutOfRangeException(nameof(elevationDegrees), "Elevation degrees must be between 0 and 90."); 

            //-- Azimuth movement
            var azDegPerStep =_aziumuthController.GetDegreesPerStep();
            int targetAzStep = (int)(azimuthDegrees / azDegPerStep);

            int azStepsToMove = targetAzStep - _currentAzStep;
            bool azForward = azStepsToMove >= 0;
            if (azStepsToMove < 0) azStepsToMove *= -1;
            if (azStepsToMove != 0)
            {
                double azTimePerStepSec = durationSeconds / azStepsToMove;
                _aziumuthController.Step(azForward, azStepsToMove, azTimePerStepSec);
                _currentAzStep = targetAzStep;
            }

            //-- Elevation movement
            var elDegPerStep = _elevationController.GetDegreesPerStep();
            int targetElStep = (int)(elevationDegrees / elDegPerStep);

            int elStepsToMove = targetElStep - _currentElStep;
            bool elForward = elStepsToMove >= 0;
            if (elStepsToMove < 0) elStepsToMove *= -1;
            if (elStepsToMove != 0)
            {
                double elTimePerStepSec = durationSeconds / elStepsToMove;
                _elevationController.Step(elForward, elStepsToMove, elTimePerStepSec);
                _currentElStep = targetElStep;
            }
        }

        public async Task MoveToAsync(double azimuthDegrees, double elevationDegrees, double durationSeconds = 0)
        {
            //Validate angles
            if (azimuthDegrees < 0 || azimuthDegrees >= 360) 
                throw new ArgumentOutOfRangeException(nameof(azimuthDegrees), "Azimuth degrees must be between 0 and 360.");
            if (elevationDegrees < 0 || elevationDegrees > 90) 
                throw new ArgumentOutOfRangeException(nameof(elevationDegrees), "Elevation degrees must be between 0 and 90."); 

            //-- Azimuth movement
            var azDegPerStep =_aziumuthController.GetDegreesPerStep();
            int targetAzStep = (int)(azimuthDegrees / azDegPerStep);

            int azStepsToMove = targetAzStep - _currentAzStep;
            bool azForward = azStepsToMove >= 0;
            if (azStepsToMove < 0) azStepsToMove *= -1;
            double azTimePerStepSec = durationSeconds / azStepsToMove;

            //-- Elevation movement
            var elDegPerStep = _elevationController.GetDegreesPerStep();
            int targetElStep = (int)(elevationDegrees / elDegPerStep);

            int elStepsToMove = targetElStep - _currentElStep;
            bool elForward = elStepsToMove >= 0;
            if (elStepsToMove < 0) elStepsToMove *= -1;
            double elTimePerStepSec = durationSeconds / elStepsToMove;

            // compute az/el step counts as before, then:
            var azTask = _aziumuthController.StepAsync(azForward, azStepsToMove, azTimePerStepSec);
            var elTask = _elevationController.StepAsync(elForward, elStepsToMove, elTimePerStepSec);

            await Task.WhenAll(azTask, elTask);

            _currentAzStep = targetAzStep;
            _currentElStep = targetElStep;
        }

        void IDisposable.Dispose()
        {
            _aziumuthController.Dispose();
            _elevationController.Dispose();
        }

        public (double, double) GetCurrentZElPosition()
        {
            return (_currentAzStep, _currentElStep);
        }
    }
}
