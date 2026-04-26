using System.ComponentModel;
using System.Runtime.CompilerServices;
using PedestrianCrossing.Models;

namespace PedestrianCrossing.ViewModels
{
    public class PedestrianViewModel : INotifyPropertyChanged
    {
        public Pedestrian Model { get; }

        public double X => Model.X;
        public double Y => Model.Y;
        public double Size => Model.Size;

        public PedestrianViewModel(Pedestrian pedestrian)
        {
            Model = pedestrian;
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