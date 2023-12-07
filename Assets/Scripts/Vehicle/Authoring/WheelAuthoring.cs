using Unity.Entities;
using UnityEngine;

public class WheelAuthoring : MonoBehaviour
{
    public float maxTraction;

    public float traction;

    class Baking : Baker<WheelAuthoring>
    {
        public override void Bake(WheelAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            WheelComponent wheelComponent = new() { maxTraction = authoring.maxTraction, traction = authoring.traction };

            AddComponent(entity, wheelComponent);
        }
    }
}