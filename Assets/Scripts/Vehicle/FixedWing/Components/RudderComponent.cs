using Unity.Entities;

public partial struct RudderComponent : IComponentData, ComponentId
{
    public int Id { get; set; }

    public float area;

    public float currentRudderAngleDegrees;

    public float maxRudderAngleDegrees;
}
