using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

/*public partial class LiftAuthoring : MonoBehaviour
{
    public float fixedWingTopArea;

    public List<LiftCoefficientValue> liftCoefficientAoAValues;

    class Baking : Baker<LiftAuthoring>
    {
        public override void Bake(LiftAuthoring authoring)
        {
            FixedWingLiftComponent fixedWingLiftComponent = new FixedWingLiftComponent();

            float smallestLiftCoefficient = FindSmallestLiftCoefficient(authoring.liftCoefficientAoAValues);
            float largestLiftCoefficient = FindLargestLiftCoefficient(authoring.liftCoefficientAoAValues);

            float totalLiftCoefficient = largestLiftCoefficient - smallestLiftCoefficient;

            fixedWingLiftComponent.topArea = authoring.fixedWingTopArea;
            fixedWingLiftComponent.maxCoefficientLift = largestLiftCoefficient;
            fixedWingLiftComponent.minCoefficientLift = smallestLiftCoefficient;
            fixedWingLiftComponent.pitchLiftCurve = new HighFidelityFixedAnimationCurve();

            fixedWingLiftComponent.pitchLiftCurve.SetCurve(CreateCurveFromLiftCoefficients(authoring.liftCoefficientAoAValues, smallestLiftCoefficient, totalLiftCoefficient));

            AddComponent(GetEntity(TransformUsageFlags.Dynamic), fixedWingLiftComponent);
        }

        private AnimationCurve CreateCurveFromLiftCoefficients(List<LiftCoefficientValue> liftCoefficients, float smallestLiftCoefficient, float totalLiftCoefficient)
        {
            AnimationCurve animationCurve = new AnimationCurve();

            foreach (LiftCoefficientValue liftCoefficientValue in liftCoefficients)
            {
                animationCurve.AddKey((liftCoefficientValue.angleOfAttack + 90f) / 180f, (liftCoefficientValue.liftCoefficient - smallestLiftCoefficient) / totalLiftCoefficient);
            }

            return animationCurve;
        }

        private float FindLargestLiftCoefficient(List<LiftCoefficientValue> liftCoefficients)
        {
            float largestLiftCoefficient = 0f;

            foreach (LiftCoefficientValue liftCoefficientValue in liftCoefficients)
            {
                if (liftCoefficientValue.angleOfAttack <= 0) continue;

                if (liftCoefficientValue.liftCoefficient < largestLiftCoefficient) return largestLiftCoefficient;
                
                largestLiftCoefficient = liftCoefficientValue.liftCoefficient;
            }

            return largestLiftCoefficient;
        }

        private float FindSmallestLiftCoefficient(List<LiftCoefficientValue> liftCoefficients)
        {
            float smallest = float.MaxValue;

            foreach (LiftCoefficientValue liftCoefficientValue in liftCoefficients)
            {
                if (liftCoefficientValue.liftCoefficient > smallest) return smallest;

                smallest = liftCoefficientValue.liftCoefficient;
            }

            return 0;
        }
    }
}
*/
