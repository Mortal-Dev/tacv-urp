using System;
using UnityEngine;
using Unity.Entities;

public class CenterOfPressureAuthoring : MonoBehaviour
{

    class Baking : Baker<CenterOfPressureAuthoring>
    {
        public override void Bake(CenterOfPressureAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new CenterOfPressureComponent());
        }
    }

    [Serializable]
    public class AoALiftCoefficientPercentage
    {
        [Range(-90f, 90f)] public float AoA;
        [Range(0f, 1.0f)] public float LiftCoefficientPercentage;
    }
}