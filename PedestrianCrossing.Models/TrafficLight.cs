using System;

namespace PedestrianCrossing.Models
{
    public class TrafficLight
    {
        private readonly double _greenDuration;
        private readonly double _redDuration;
        private double _elapsed;

        public bool IsGreenForPedestrian { get; private set; }


        public event EventHandler<bool>? GreenLightChanged;

        public TrafficLight(double greenSeconds, double redSeconds)
        {
            _greenDuration = greenSeconds;
            _redDuration = redSeconds;
            IsGreenForPedestrian = true;
            _elapsed = 0;
        }

        
        public void Update(double deltaTime)
        {
            _elapsed += deltaTime;

            double currentCycle = IsGreenForPedestrian ? _greenDuration : _redDuration;
            if (_elapsed >= currentCycle)
            {
                _elapsed -= currentCycle;
                IsGreenForPedestrian = !IsGreenForPedestrian;
                GreenLightChanged?.Invoke(this, IsGreenForPedestrian);
            }
        }
    }
}