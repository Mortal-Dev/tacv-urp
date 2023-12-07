using Unity.Entities;

public partial struct RequestVehicleLeaveComponent : IComponentData
{
    public ulong vehicleNetworkId;
    public int seat;
}