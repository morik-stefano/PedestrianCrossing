using System;

namespace PedestrianCrossing.Models
{
    public class Car : Vehicle
    {
        public bool IsIgnoringLight { get; private set; }
        public bool CanMove { get; set; } = true;

        private bool _decisionMade;
        private readonly Random _random = new Random();

        public Car(double startX, double startY, double speed, double width = 80, double height = 30)
            : base(startX, startY, speed, width, height)
        {
        }

        public void UpdateDecision(double probability, double crossingLeft, double crossingRight)
        {
            if (Speed > 0) 
            {
                if (!_decisionMade && X + Width >= crossingLeft)
                {
                    IsIgnoringLight = _random.NextDouble() < probability;
                    _decisionMade = true;
                }
                if (X > crossingRight)     
                {
                    IsIgnoringLight = false;
                    _decisionMade = false;
                }
            }
            else      
            {
                if (!_decisionMade && X <= crossingRight)
                {
                    IsIgnoringLight = _random.NextDouble() < probability;
                    _decisionMade = true;
                }
                if (X + Width < crossingLeft)
                {
                    IsIgnoringLight = false;
                    _decisionMade = false;
                }
            }
        }

        public override void Move(double deltaTime)
        {
            if (CanMove)
                X += Speed * deltaTime;
        }
    }
}