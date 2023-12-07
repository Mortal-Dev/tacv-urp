using Unity.Entities;
using UnityEngine;

public class FixedWingAuthoring : MonoBehaviour
{
    public string aircraftName;

    public string aircraftBerevityCode;

    class Baking : Baker<FixedWingAuthoring>
    {
        public override void Bake(FixedWingAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new FixedWingComponent());
            AddComponent(entity, new FixedWingInputComponent() { throttle = 1f });
            AddComponent(entity, new UninitializedFixedWingComponent());
        }
    }
}