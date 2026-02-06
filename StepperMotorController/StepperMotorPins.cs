using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepperMotorController
{
    public class StepperMotorPins
    {
        public StepperMotorPins(int stepPin, int dirPin, int m0Pin, int m1Pin, int m2Pin)
        {
            StepPin = stepPin;
            DirPin = dirPin;
            M0Pin = m0Pin;
            M1Pin = m1Pin;
            M2Pin = m2Pin;
        }

        public int StepPin { get; private set; }
        public int DirPin { get; private set; }
        public int M0Pin { get; private set; }
        public int M1Pin { get; private set; }
        public int M2Pin { get; private set; }
    }
}
