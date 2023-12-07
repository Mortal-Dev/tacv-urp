using Unity.Entities;
using UnityEngine;

public partial struct FixedWingLiftComponent : IComponentData
{
    public float topArea;
    public float maxCoefficientLift;
    public float minCoefficientLift;

    public HighFidelityFixedAnimationCurve pitchLiftCurve;
    public LowFidelityFixedAnimationCurve yawLiftCurve;
}