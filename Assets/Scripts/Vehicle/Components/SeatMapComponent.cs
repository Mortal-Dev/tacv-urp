using Unity.Collections;
using Unity.Entities;

public partial struct SeatMapComponent : IComponentData
{
    public FixedList128Bytes<Entity> seats;

    public Entity currentSeatWithOwnership;
}