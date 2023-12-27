using System;
using Unity.Entities;
using Unity.XR.CoreUtils;
using UnityEngine;

public class WheelAuthoring : MonoBehaviour
{
    public float radius;

    public float wheelWeight;

    public WheelDirectionTractionValue[] wheelTractionCurve;

    class Baking : Baker<WheelAuthoring>
    {
        public override void Bake(WheelAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            WheelComponent wheelComponent = new() { radius = authoring.radius, wheelWeight = authoring.wheelWeight, 
                tractionCurve = ConvertWheelDirectionTractionValueToFixedCurve(authoring.wheelTractionCurve) };

            AddComponent(entity, wheelComponent);
        }

        private LowFidelityFixedAnimationCurve ConvertWheelDirectionTractionValueToFixedCurve(WheelDirectionTractionValue[] values)
        {
            AnimationCurve animationCurve = new();

            foreach (WheelDirectionTractionValue wheelDirectionTractionValue in values)
            {
                animationCurve.AddKey(wheelDirectionTractionValue.wheelSideToFowardRatio, wheelDirectionTractionValue.traction);
            }

            LowFidelityFixedAnimationCurve lowFidelityFixedAnimationCurve = new();

            lowFidelityFixedAnimationCurve.SetCurve(animationCurve);

            return lowFidelityFixedAnimationCurve;
        }
    }

    [Serializable]
    public class WheelDirectionTractionValue
    {
        [Range(0f, 1f)]
        public float wheelSideToFowardRatio;

        public float traction;
    }
}