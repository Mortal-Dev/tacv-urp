using Unity.Entities;

public partial struct RequestVehicleEnterComponent : IComponentData
{
    public ulong vehicleNetworkId;
    public int seat;
}