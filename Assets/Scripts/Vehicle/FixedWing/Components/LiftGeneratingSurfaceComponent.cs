using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial struct LiftGeneratingSurfaceComponent : IComponentData, ComponentId
{
    public int Id { get; set; }

    public float liftArea;

    public LowFidelityFixedAnimationCurve PitchAoALiftCoefficientCurve;

    public LowFidelityFixedAnimationCurve YawAoALiftCoefficientCurve;

    public Entity liftEntity;

    public LocalTransform lastGlobalTransform;

    public float3 lastLocalPosition;
}