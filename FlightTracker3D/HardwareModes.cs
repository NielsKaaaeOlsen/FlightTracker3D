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
        public HardwareModeEnum LcdHardwareMode { get; private set; }
        public HardwareModeEnum LedHardwareMode { get; private set; }
        public HardwareModeEnum StepperMotorHardwareMode { get; private set; }
        public HardwareModeEnum AirCraftListenerMode { get; private set; }

        public HardwareModes(HardwareModeEnum lcdHardwareMode, HardwareModeEnum ledHardwareMode, HardwareModeEnum stepperMotorHardwareMode, HardwareModeEnum airCraftListenerMode)
        {
            LcdHardwareMode = lcdHardwareMode;
            LedHardwareMode = ledHardwareMode;
            StepperMotorHardwareMode = stepperMotorHardwareMode;
            AirCraftListenerMode = airCraftListenerMode;
        }
    }
}
