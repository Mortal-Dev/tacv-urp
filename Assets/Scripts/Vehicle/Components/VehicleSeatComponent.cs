using Unity.Entities;

public partial struct VehicleSeatComponent : IComponentData
{
    public int seatPosition;

    public bool isOccupied;

    public Entity occupiedBy;

    public bool hasOwnership;

    public bool hasOwnershipCapability;
}