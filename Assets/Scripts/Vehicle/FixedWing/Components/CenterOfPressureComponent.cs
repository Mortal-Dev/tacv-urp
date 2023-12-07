using Unity.Entities;
using Unity.Mathematics;

public partial struct CenterOfPressureComponent : IComponentData
{
    public Entity maxCenterOfPressureEntity;
    public Entity minCenterOfPressureEntity;

    public LowFidelityFixedAnimationCurve centerOfPressureCurve;
}