using Unity.Entities;
using Unity.Collections;

public partial struct VehicleComponent : IComponentData
{
    public FixedList128Bytes<Entity> seats;

    public Entity currentSeatWithOwnership;

    public FixedList128Bytes<Entity> wheels;
}