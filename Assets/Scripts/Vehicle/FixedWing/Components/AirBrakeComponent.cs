using Unity.Entities;

public partial struct AirBrakeComponent : IComponentData, ComponentId
{
    public float maxDrag;

    public float timeToDeploy;

    public int Id { get; set; }
}
