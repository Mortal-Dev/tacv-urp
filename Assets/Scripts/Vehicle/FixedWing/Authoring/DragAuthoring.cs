using UnityEngine;
using Unity.Entities;
using System;

public class DragAuthoring : MonoBehaviour
{
    public float forwardProjectedArea;
    public DragCoefficientValue[] forwardDragAoACoefficients;

    public float backProjectedArea;
    public DragCoefficientValue[] backwardDragAoACoefficients;

    public float sideProjectedArea;
    public DragCoefficientValue[] sideDragAoACoefficients;

    class Baking : Baker<DragAuthoring>
    {
        public override void Bake(DragAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            FixedWingDragComponent fixedWingDragComponent = new FixedWingDragComponent();

            fixedWingDragComponent.forwardArea = authoring.forwardProjectedArea;
            fixedWingDragComponent.maxForwardDragCoefficient = authoring.forwardDragAoACoefficients[^1].dragCoefficient;
            fixedWingDragComponent.forwardDragCoefficientAoACurve = new LowFidelityFixedAnimationCurve();
            fixedWingDragComponent.forwardDragCoefficientAoACurve.SetCurve(CreateCurveFromDragCoefficients(authoring.forwardDragAoACoefficients));

            fixedWingDragComponent.backArea = authoring.backProjectedArea;
            fixedWingDragComponent.maxBackDragCoefficient = authoring.backwardDragAoACoefficients[^1].dragCoefficient;
            fixedWingDragComponent.backDragCoefficientAoACurve = new LowFidelityFixedAnimationCurve();
            fixedWingDragComponent.backDragCoefficientAoACurve.SetCurve(CreateCurveFromDragCoefficients(authoring.backwardDragAoACoefficients));

            fixedWingDragComponent.rightSideArea = authoring.sideProjectedArea;
            fixedWingDragComponent.maxRightSideDragCoefficient = authoring.sideDragAoACoefficients[^1].dragCoefficient;
            fixedWingDragComponent.rightSideDragCoefficientAoACurve = new LowFidelityFixedAnimationCurve();
            fixedWingDragComponent.rightSideDragCoefficientAoACurve.SetCurve(CreateCurveFromDragCoefficients(authoring.sideDragAoACoefficients));

            fixedWingDragComponent.leftSideArea = authoring.sideProjectedArea;
            fixedWingDragComponent.maxLeftSideDragCoefficient = authoring.sideDragAoACoefficients[^1].dragCoefficient;
            fixedWingDragComponent.leftSideDragCoefficientAoACurve = new LowFidelityFixedAnimationCurve();
            fixedWingDragComponent.leftSideDragCoefficientAoACurve.SetCurve(CreateCurveFromDragCoefficients(authoring.sideDragAoACoefficients));


            AddComponent(entity, fixedWingDragComponent);
        }

        private AnimationCurve CreateCurveFromDragCoefficients(DragCoefficientValue[] dragCoefficientInserts)
        {
            AnimationCurve animationCurve = new AnimationCurve();

            foreach (DragCoefficientValue dragCoefficientValue in dragCoefficientInserts)
            {
                animationCurve.AddKey(dragCoefficientValue.angleOfAttack / 180, dragCoefficientValue.dragCoefficient);
            }

            return animationCurve;
        }
    }
}

[Serializable]
public class DragCoefficientValue
{
    [Range(-90f, 90f)] public float angleOfAttack;
    public float dragCoefficient;
}
