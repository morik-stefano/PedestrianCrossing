using System;

namespace PedestrianCrossing.Models
{
    public class EmergencyEventArgs : EventArgs
    {
        public double AccidentX { get; }
        public double AccidentY { get; }
        public DateTime Time { get; }

        public EmergencyEventArgs(double x, double y)
        {
            AccidentX = x;
            AccidentY = y;
            Time = DateTime.Now;
        }
    }
}