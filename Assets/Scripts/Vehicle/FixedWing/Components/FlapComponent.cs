using Unity.Entities;

public partial struct FlapComponent : IComponentData, ComponentId
{
    public int Id { get; set; }

    public float currentFlapDegree;

    public float currentDrag;

    public float liftProvided;

    public float maxFlapDegree;

    public float maxDrag;

    public float maxLift;
}
