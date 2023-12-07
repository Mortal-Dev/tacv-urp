using UnityEngine;
using Unity.Entities;
using System;

public class LiftGeneratingSurfaceAuthoring : MonoBehaviour
{
    public int positionId;

    public float liftArea;

    public LiftCoefficientValue[] AoALiftCoefficients;

    class Baking : Baker<LiftGeneratingSurfaceAuthoring>
    {
        public override void Bake(LiftGeneratingSurfaceAuthoring authoring)
        {
            LowFidelityFixedAnimationCurve lowFidelityFixedAnimationCurve = new LowFidelityFixedAnimationCurve();

            lowFidelityFixedAnimationCurve.SetCurve(CreateAnimationCurveFromAoALiftCoefficientValues(authoring.AoALiftCoefficients));

            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new LiftGeneratingSurfaceComponent() { Id = authoring.positionId, liftArea = authoring.liftArea, PitchAoALiftCoefficientCurve = lowFidelityFixedAnimationCurve });
        }

        private AnimationCurve CreateAnimationCurveFromAoALiftCoefficientValues(LiftCoefficientValue[] aoALiftCoefficientValues)
        {
            AnimationCurve animationCurve = new();

            foreach (LiftCoefficientValue value in aoALiftCoefficientValues)
            {
                animationCurve.AddKey(value.angleOfAttack + 90f / 180f, value.liftCoefficient);
            }

            return animationCurve;
        }
    }
}

[Serializable]
public class LiftCoefficientValue
{
    public float angleOfAttack;
    public float liftCoefficient;
}
