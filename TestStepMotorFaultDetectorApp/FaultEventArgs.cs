using System;

namespace TestStepMotorFaultDetectorApp
{
    public class FaultEventArgs : EventArgs
    {
        public DateTime Timestamp { get; }

        public FaultEventArgs(DateTime timestamp)
        {
            Timestamp = timestamp;
        }
    }
}
