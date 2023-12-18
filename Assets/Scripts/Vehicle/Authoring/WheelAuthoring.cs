using Unity.Entities;
using UnityEngine;

public class WheelAuthoring : MonoBehaviour
{
    public float radius;

    public float traction;

    public bool isTurnableWheel;

    class Baking : Baker<WheelAuthoring>
    {
        public override void Bake(WheelAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            WheelComponent wheelComponent = new() { radius = authoring.radius, traction = authoring.traction, isTurnableWheel = authoring.isTurnableWheel };

            AddComponent(entity, wheelComponent);
        }
    }
}