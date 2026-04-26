using System.ComponentModel;
using System.Runtime.CompilerServices;
using PedestrianCrossing.Models;

namespace PedestrianCrossing.ViewModels
{
    public class TrafficLightViewModel : INotifyPropertyChanged
    {
        private readonly TrafficLight _trafficLight;

        public bool IsGreen => _trafficLight.IsGreenForPedestrian;

        public TrafficLightViewModel(TrafficLight trafficLight)
        {
            _trafficLight = trafficLight;
            _trafficLight.GreenLightChanged += (s, e) => OnPropertyChanged(nameof(IsGreen));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}