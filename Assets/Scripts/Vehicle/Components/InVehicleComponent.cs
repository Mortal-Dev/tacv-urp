using Unity.Entities;

public enum OwnershipType
{
    Shared,
    Owned,
    NotOwned
}

public partial struct InVehicleComponent : IComponentData
{
    public OwnershipType ownershipType;

    // -1 means no-one is holding the throttle/stick and anyone can use it
    public int currentOwnerId;

    public Entity seat;

    public Entity vehicle;
}
