using Unity.Entities;
using UnityEngine;

public partial struct FixedWingDragComponent : IComponentData
{
    public float maxForwardDragCoefficient;
    public float forwardArea;
    public LowFidelityFixedAnimationCurve forwardDragCoefficientAoACurve;

    public float maxBackDragCoefficient;
    public float backArea;
    public LowFidelityFixedAnimationCurve backDragCoefficientAoACurve;


    public float maxLeftSideDragCoefficient;
    public float leftSideArea;
    public LowFidelityFixedAnimationCurve leftSideDragCoefficientAoACurve;

    public float maxRightSideDragCoefficient;
    public float rightSideArea;
    public LowFidelityFixedAnimationCurve rightSideDragCoefficientAoACurve;
}