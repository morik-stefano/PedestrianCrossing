using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using PedestrianCrossing.Helpers;
using PedestrianCrossing.Models;

namespace PedestrianCrossing.ViewModels
{
    public class MainViewModel
    {
        public CrossingSimulation Simulation { get; }

        public ObservableCollection<VehicleViewModel> Vehicles { get; } = new();
        public ObservableCollection<PedestrianViewModel> Pedestrians { get; } = new();
        public ObservableCollection<VehicleViewModel> EmergencyVehicles { get; } = new();

        public TrafficLightViewModel TrafficLight { get; }

        public ICommand AddCarCommand { get; }
        public ICommand AddPedestrianCommand { get; }
        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }

        public ObservableCollection<string> VehicleTypes { get; } = new();
        public string SelectedVehicleType
        {
            get => _selectedVehicleType;
            set => _selectedVehicleType = value;
        }
        private string _selectedVehicleType;

        private readonly Random _rnd = new Random();

        public MainViewModel()
        {
            var trafficLight = new TrafficLight(greenSeconds: 5, redSeconds: 7);
            var emergencyService = new EmergencyService();

            TrafficLight = new TrafficLightViewModel(trafficLight);

            Simulation = new CrossingSimulation(trafficLight, emergencyService)
            {
                IgnoreRedProbability = 0.5, // это для теста, потом можно уменьшить (игнор красного светофора водятелом) 
                UIUpdateAction = () => Application.Current.Dispatcher.Invoke(() => RefreshAllViewModels())
            };

            Simulation.EmergencyOccurred += (s, e) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Авария! Координаты: ({e.AccidentX:F0}, {e.AccidentY:F0})");
                });
            };

            AddCarCommand = new DelegateCommand(AddCar);
            AddPedestrianCommand = new DelegateCommand(AddPedestrian);
            StartCommand = new DelegateCommand(StartSimulation);
            StopCommand = new DelegateCommand(StopSimulation);

            LoadVehicleTypes();
        }

        private void LoadVehicleTypes()
        {
            var assembly = Assembly.GetAssembly(typeof(Vehicle));
            if (assembly == null) return;

            var types = assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Vehicle)) && !t.IsAbstract && t != typeof(EmergencyVehicle))
                .Select(t => t.Name)
                .ToList();

            VehicleTypes.Clear();
            foreach (var t in types)
                VehicleTypes.Add(t);
            SelectedVehicleType = VehicleTypes.FirstOrDefault();
        }

        private void AddCar()
        {
            double startX, startY, speed;
            bool rightLane = _rnd.Next(2) == 0;
            if (rightLane)
            {
                startX = -80;
                startY = 130;  
                speed = 90;
            }
            else
            {
                startX = 850;
                startY = 160; 
                speed = -70;
            }

            Vehicle newVehicle = null;
            if (!string.IsNullOrEmpty(SelectedVehicleType))
            {
                var assembly = Assembly.GetAssembly(typeof(Vehicle));
                var type = assembly.GetTypes()
                    .FirstOrDefault(t => t.Name == SelectedVehicleType && t.IsSubclassOf(typeof(Vehicle)));
                if (type != null)
                {
                    newVehicle = (Vehicle)Activator.CreateInstance(type, startX, startY, speed, 80.0, 30.0);
                }
            }

            if (newVehicle is Car car)
            {
                Simulation.Cars.Add(car);
                Vehicles.Add(new VehicleViewModel(car));
            }
            else
            {
                // fallback
                var fallbackCar = new Car(startX, startY, speed);
                Simulation.Cars.Add(fallbackCar);
                Vehicles.Add(new VehicleViewModel(fallbackCar));
            }
        }

        private void AddPedestrian()
        {
            double x = 350 + _rnd.NextDouble() * 100; 
            double y = 350;
            var ped = new Pedestrian(x, y, speed: 30, size: 20);
            Simulation.Pedestrians.Add(ped);
            Pedestrians.Add(new PedestrianViewModel(ped));
        }

        private void RefreshAllViewModels()
        {
            for (int i = Vehicles.Count - 1; i >= 0; i--)
            {
                if (!Simulation.Cars.Contains(Vehicles[i].Model))
                    Vehicles.RemoveAt(i);
            }

            for (int i = Pedestrians.Count - 1; i >= 0; i--)
            {
                if (!Simulation.Pedestrians.Contains(Pedestrians[i].Model))
                    Pedestrians.RemoveAt(i);
            }

            foreach (var ev in Simulation.EmergencyVehicles)
            {
                if (!EmergencyVehicles.Any(vm => vm.Model == ev))
                    EmergencyVehicles.Add(new VehicleViewModel(ev));
            }
            for (int i = EmergencyVehicles.Count - 1; i >= 0; i--)
            {
                if (!Simulation.EmergencyVehicles.Contains(EmergencyVehicles[i].Model))
                    EmergencyVehicles.RemoveAt(i);
            }

            foreach (var vm in Vehicles) vm.RefreshPosition();
            foreach (var vm in Pedestrians) vm.RefreshPosition();
            foreach (var vm in EmergencyVehicles) vm.RefreshPosition();
        }

        private void StartSimulation() => Simulation.Start();
        private void StopSimulation() => Simulation.Stop();
    }
}