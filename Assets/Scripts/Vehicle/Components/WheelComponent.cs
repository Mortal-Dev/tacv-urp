using Unity.Entities;

public partial struct WheelComponent : IComponentData
{
    public float radius;

    public float wheelWeight;

    public LowFidelityFixedAnimationCurve tractionCurve;

    public float rpm;
}