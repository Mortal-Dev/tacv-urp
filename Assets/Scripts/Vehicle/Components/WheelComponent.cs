using Unity.Entities;

public partial struct WheelComponent : IComponentData
{
    public bool isTurnableWheel;

    public float maxTraction;

    public float traction;

    public float rpm;
}