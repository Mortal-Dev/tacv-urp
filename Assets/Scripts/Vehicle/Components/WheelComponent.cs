using Unity.Entities;

public partial struct WheelComponent : IComponentData
{
    public float radius;

    public bool isTurnableWheel;

    public float traction;

    public float rpm;
}