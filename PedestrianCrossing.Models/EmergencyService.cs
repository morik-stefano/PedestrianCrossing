using System;

namespace PedestrianCrossing.Models
{
    public class EmergencyService : IEmergencyService
    {
        public event EventHandler<EmergencyEventArgs>? EmergencyVehicleDispatched;

        public void SendVehicle(double x, double y)
        {
            EmergencyVehicleDispatched?.Invoke(this, new EmergencyEventArgs(x, y));
        }
    }

    public class EmergencyVehicle : Vehicle
    {
        public double TargetX { get; set; }
        public double TargetY { get; set; }

        public EmergencyVehicle(double startX, double startY, double targetX, double targetY)
            : base(startX, startY, speed: 100, width: 90, height: 35)
        {
            TargetX = targetX;
            TargetY = targetY;
        }

        public override void Move(double deltaTime)
        {
            double dx = TargetX - X;
            double dy = TargetY - Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);
            if (distance < 2) return;
            double step = Speed * deltaTime;
            if (step > distance) step = distance;
            X += dx / distance * step;
            Y += dy / distance * step;
        }
    }
}