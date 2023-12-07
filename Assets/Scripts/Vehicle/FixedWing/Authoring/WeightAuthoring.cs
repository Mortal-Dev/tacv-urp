using Unity.Entities;
using UnityEngine;

internal class WeightAuthoring : MonoBehaviour
{
    [SerializeField]
    private float weightKilograms;

    class Baking : Baker<WeightAuthoring> 
    {
        public override void Bake(WeightAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new WeightComponent() { weightKilograms = authoring.weightKilograms });
        }
    }
}