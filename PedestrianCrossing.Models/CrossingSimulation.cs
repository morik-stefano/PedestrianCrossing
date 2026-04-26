using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PedestrianCrossing.Models
{
    public class CrossingSimulation
    {
        public List<Car> Cars { get; } = new List<Car>();
        public List<Pedestrian> Pedestrians { get; } = new List<Pedestrian>();
        public List<EmergencyVehicle> EmergencyVehicles { get; } = new List<EmergencyVehicle>();
        public TrafficLight TrafficLight { get; }
        public EmergencyService EmergencyService { get; }

        public event EventHandler<EmergencyEventArgs>? EmergencyOccurred;

        public double IgnoreRedProbability { get; set; } = 0.08;

        public double CrossingLeft => 300;
        public double CrossingRight => 500;
        public double CrossingY => 120;

        private const double CanvasWidth = 800;
        private const double CanvasHeight = 400;

        private CancellationTokenSource? _cts;
        private Task? _simulationTask;

        // Состояние аварии
        private volatile bool _accidentInProgress;
        private Car? _accidentCar;
        private Pedestrian? _accidentPedestrian;
        private EmergencyVehicle? _accidentServiceVehicle;
        private DateTime _accidentStartTime;
        private const double AccidentDurationSeconds = 4.0;

        public Action? UIUpdateAction { get; set; }

        public CrossingSimulation(TrafficLight trafficLight, EmergencyService emergencyService)
        {
            TrafficLight = trafficLight;
            EmergencyService = emergencyService;
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            _simulationTask = Task.Run(() => SimulationLoop(_cts.Token));
        }

        public void Stop()
        {
            _cts?.Cancel();
            _simulationTask?.Wait(1000);
        }

        private async Task SimulationLoop(CancellationToken token)
        {
            DateTime lastTime = DateTime.Now;
            while (!token.IsCancellationRequested)
            {
                DateTime now = DateTime.Now;
                double deltaTime = (now - lastTime).TotalSeconds;
                lastTime = now;
                if (deltaTime > 0.1) deltaTime = 0.1;

                if (_accidentInProgress)
                {
                    _accidentServiceVehicle?.Move(deltaTime);

                    bool serviceArrived = _accidentServiceVehicle != null &&
                        Math.Abs(_accidentServiceVehicle.X - _accidentServiceVehicle.TargetX) < 3 &&
                        Math.Abs(_accidentServiceVehicle.Y - _accidentServiceVehicle.TargetY) < 3;

                    bool timeExpired = (DateTime.Now - _accidentStartTime).TotalSeconds >= AccidentDurationSeconds;

                    if (serviceArrived || timeExpired)
                    {
                        if (_accidentCar != null) Cars.Remove(_accidentCar);
                        if (_accidentPedestrian != null) Pedestrians.Remove(_accidentPedestrian);
                        if (_accidentServiceVehicle != null) EmergencyVehicles.Remove(_accidentServiceVehicle);

                        _accidentCar = null;
                        _accidentPedestrian = null;
                        _accidentServiceVehicle = null;
                        _accidentInProgress = false;
                    }

                    UIUpdateAction?.Invoke();
                    await Task.Delay(30, token);
                    continue;
                }

                TrafficLight.Update(deltaTime);

                foreach (var car in Cars)
                {
                    car.UpdateDecision(IgnoreRedProbability, CrossingLeft, CrossingRight);
                    UpdateCarMovement(car, deltaTime);
                    car.Move(deltaTime);
                }

                foreach (var ped in Pedestrians)
                {
                    if (TrafficLight.IsGreenForPedestrian)
                        ped.IsCrossing = true;
                    ped.Move(deltaTime);
                }

                foreach (var ev in EmergencyVehicles)
                    ev.Move(deltaTime);

                CheckCollisions();

                Cars.RemoveAll(c => c.X > CanvasWidth + 100 || c.X < -100);
                Pedestrians.RemoveAll(p => p.Y < 50);

                UIUpdateAction?.Invoke();
                await Task.Delay(30, token);
            }
        }

        private void UpdateCarMovement(Car car, double deltaTime)
        {
            bool isRedForCars = TrafficLight.IsGreenForPedestrian;
            double stopLineLeft = CrossingLeft - car.Width;
            double stopLineRight = CrossingRight;

            if (car.Speed > 0)
            {
                if (isRedForCars && !car.IsIgnoringLight)
                {
                    if (car.X < stopLineLeft && car.X + car.Speed * deltaTime >= stopLineLeft)
                        car.CanMove = false;
                    else if (car.X >= stopLineLeft && car.X < stopLineRight)
                        car.CanMove = true;
                    else
                        car.CanMove = true;
                }
                else
                {
                    car.CanMove = true;
                }
            }
            else
            {
                if (isRedForCars && !car.IsIgnoringLight)
                {
                    if (car.X > stopLineRight && car.X + car.Speed * deltaTime <= stopLineRight)
                        car.CanMove = false;
                    else if (car.X <= stopLineRight && car.X > stopLineLeft)
                        car.CanMove = true;
                    else
                        car.CanMove = true;
                }
                else
                {
                    car.CanMove = true;
                }
            }
        }

        private void CheckCollisions()
        {
            for (int i = Pedestrians.Count - 1; i >= 0; i--)
            {
                var ped = Pedestrians[i];
                if (!ped.IsCrossing) continue;

                double pedLeft = ped.X;
                double pedTop = ped.Y;
                double pedRight = ped.X + ped.Size;
                double pedBottom = ped.Y + ped.Size;

                for (int j = Cars.Count - 1; j >= 0; j--)
                {
                    var car = Cars[j];

                    bool carInCrossing = car.X + car.Width > CrossingLeft && car.X < CrossingRight;
                    if (!carInCrossing) continue;

                    double carLeft = car.X;
                    double carTop = car.Y;
                    double carRight = car.X + car.Width;
                    double carBottom = car.Y + car.Height;

                    bool collision = carLeft < pedRight &&
                                     carRight > pedLeft &&
                                     carTop < pedBottom &&
                                     carBottom > pedTop;

                    if (collision)
                    {
                        StartAccident(car, ped);
                        return;
                    }
                }
            }
        }

        private void StartAccident(Car car, Pedestrian ped)
        {
            // Замораживаем виновников
            car.CanMove = false;
            ped.IsCrossing = false; 

            _accidentCar = car;
            _accidentPedestrian = ped;
            _accidentStartTime = DateTime.Now;
            _accidentInProgress = true;

            // Создаём аварийную машину
            double targetX = ped.X + ped.Size / 2;
            double targetY = ped.Y + ped.Size / 2;
            _accidentServiceVehicle = new EmergencyVehicle(startX: 0, startY: 50, targetX: targetX, targetY: targetY);
            EmergencyVehicles.Add(_accidentServiceVehicle);

            EmergencyOccurred?.Invoke(this, new EmergencyEventArgs(targetX, targetY));
        }
    }
}