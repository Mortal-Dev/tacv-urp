using Unity.Entities;
using Unity.Collections;
using System.Linq;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(FixedWingStateSystem))]
[UpdateAfter(typeof(VehicleInitializeSystem))]
public partial class FixedWingInitializeSystem : SystemBase
{
    protected override void OnUpdate()
    {
        EntityCommandBuffer entityCommandBuffer = new(Allocator.Temp);

        Entities.ForEach((Entity entity, ref FixedWingComponent fixedWingComponent, in UninitializedFixedWingComponent uninitializedfixedWingComponent) =>
        {
            entityCommandBuffer.RemoveComponent<UninitializedFixedWingComponent>(entity);

            DynamicBuffer<LinkedEntityGroup> children = EntityManager.GetBuffer<LinkedEntityGroup>(entity);

            SetFixedWingEntities(ref fixedWingComponent, children);
            
        }).WithoutBurst().Run();

        entityCommandBuffer.Playback(EntityManager);
        entityCommandBuffer.Dispose();
    }
    
    private void SetCenterOfMass(DynamicBuffer<LinkedEntityGroup> children)
    {
        float3 meanMassComponentPosition = float3.zero;

        foreach (LinkedEntityGroup child in children)
        {
            if (!SystemAPI.HasComponent<WeightComponent>(child.Value)) continue;

            LocalTransform localTransform = SystemAPI.GetComponent<LocalTransform>(child.Value);
        }
    }

    //looks slow, but takes less than a millisecond, and only runs once when the aircraft is initialized, but I could probably make a more performant version later
    private void SetFixedWingEntities(ref FixedWingComponent fixedWingComponent, DynamicBuffer<LinkedEntityGroup> children)
    {
        fixedWingComponent.flapEntities = GetEntities<FlapComponent>(children);
        fixedWingComponent.engineEntities = GetEntities<EngineComponent>(children);
        fixedWingComponent.rudderEntities = GetEntities<RudderComponent>(children);
        fixedWingComponent.stabilatorEntities = GetEntities<StabilatorComponent>(children);
        fixedWingComponent.airleronEntities = GetEntities<AirleronComponent>(children);
        fixedWingComponent.engineEntities = GetEntities<EngineComponent>(children);
        fixedWingComponent.liftGeneratingSurfaceEntities = GetEntities<LiftGeneratingSurfaceComponent>(children);

        fixedWingComponent.centerOfPressureEntity = GetEntity<CenterOfPressureComponent>(children);
        fixedWingComponent.centerOfGravityEntity = GetEntity<CenterOfGravityComponent>(children);
    }

    private Entity GetEntity<T>(DynamicBuffer<LinkedEntityGroup> children) where T : unmanaged, IComponentData
    {
        foreach (LinkedEntityGroup linkedEntityGroup in children)
        {
            if (!EntityManager.HasComponent<T>(linkedEntityGroup.Value)) continue;

            return linkedEntityGroup.Value;
        }

        return Entity.Null;
    }

    private FixedList128Bytes<Entity> GetEntities<T>(DynamicBuffer<LinkedEntityGroup> children) where T : unmanaged, IComponentData, ComponentId
    {
        FixedList128Bytes<Entity> entities = new();

        foreach (LinkedEntityGroup linkedEntityGroup in children)
        {
            if (!EntityManager.HasComponent<T>(linkedEntityGroup.Value)) continue;

            ComponentId componentId = EntityManager.GetComponentData<T>(linkedEntityGroup.Value);

            entities.Add(linkedEntityGroup.Value);
        }

        entities.OrderBy(entity => EntityManager.GetComponentData<T>(entity).Id);

        return entities;
    }
}