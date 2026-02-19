using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HardwareMode;

namespace FlightTracker3D
{
    public class HardwareModes
    {
        public HardwareMode.HardwareMode LcdHardwareMode { get; private set; }
        public HardwareMode.HardwareMode LedHardwareMode { get; private set; }
        public HardwareMode.HardwareMode StepperMotorHardwareMode { get; private set; }

        public HardwareModes(HardwareMode.HardwareMode lcdHardwareMode, HardwareMode.HardwareMode ledHardwareMode, HardwareMode.HardwareMode stepperMotorHardwareMode)
        {
            LcdHardwareMode = lcdHardwareMode;
            LedHardwareMode = ledHardwareMode;
            StepperMotorHardwareMode = stepperMotorHardwareMode;
        }
    }
}
