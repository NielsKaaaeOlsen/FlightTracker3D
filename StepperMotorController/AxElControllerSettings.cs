using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepperMotorController
{
    public class AxElControllerSettings
    {
        public required LogLevel MinimumLogLevel { get; set; }
        public required HardwareMode.HardwareMode HardwareMode { get; set; }
        public required MicrosteppingMode MicrosteppingMode { get; set; }

        public required List<MoveToCommand> MoveToCommands { get; set; }
    }

    public class MoveToCommand
    {
        public double Az { get; set; }
        public double El { get; set; }
        public double Duration { get; set; }
    }
}
