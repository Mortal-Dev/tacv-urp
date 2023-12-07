using UnityEngine;
using Unity.Entities;

public class EngineAuthoring : MonoBehaviour
{
    public int positionId;

    public float maxMilitaryPowerNewtons;

    public float maxAfterBurnerNewtons;

    class Baking : Baker<EngineAuthoring>
    {
        public override void Bake(EngineAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            EngineComponent engineComponent = new EngineComponent() { Id = authoring.positionId, maxMilitaryPowerNewtons = authoring.maxMilitaryPowerNewtons, maxAfterBurnerPowerNewtons = authoring.maxAfterBurnerNewtons };

            AddComponent(entity, engineComponent);
        }
    }
}