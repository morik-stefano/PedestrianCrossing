namespace PedestrianCrossing.Models
{
    public class Pedestrian
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Speed { get; }
        public double Size { get; }
        public bool IsCrossing { get; set; }

        public Pedestrian(double startX, double startY, double speed = 30, double size = 20)
        {
            X = startX;
            Y = startY;
            Speed = speed;
            Size = size;
            IsCrossing = false;
        }

        public void Move(double deltaTime)
        {
            if (IsCrossing)
                Y -= Speed * deltaTime; 
        }
    }
}