using System.ComponentModel;
using System.Runtime.CompilerServices;
using PedestrianCrossing.Models;

namespace PedestrianCrossing.ViewModels
{
    public class VehicleViewModel : INotifyPropertyChanged
    {
        public Vehicle Model { get; }

        public double X => Model.X;
        public double Y => Model.Y;
        public double Width => Model.Width;
        public double Height => Model.Height;

        public VehicleViewModel(Vehicle vehicle)
        {
            Model = vehicle;
        }

        public void RefreshPosition()
        {
            OnPropertyChanged(nameof(X));
            OnPropertyChanged(nameof(Y));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}