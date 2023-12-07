using Unity.Entities;
using Unity.Mathematics;

internal partial struct FixedWingInputComponent : IComponentData
{
    public float2 stickInput;
    public float throttle;
}