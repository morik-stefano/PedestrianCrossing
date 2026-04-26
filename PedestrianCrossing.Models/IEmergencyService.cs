namespace PedestrianCrossing.Models
{
    public interface IEmergencyService
    {
        event EventHandler<EmergencyEventArgs> EmergencyVehicleDispatched;
        void SendVehicle(double x, double y);
    }
}