namespace PedestrianCrossing.Models
{
    public abstract class Vehicle
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Speed { get; }
        public double Width { get; }
        public double Height { get; }

        protected Vehicle(double startX, double startY, double speed, double width, double height)
        {
            X = startX;
            Y = startY;
            Speed = speed;
            Width = width;
            Height = height;
        }

        public abstract void Move(double deltaTime);
    }
}