using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial class VehicleInitializeSystem : SystemBase
{
    protected override void OnUpdate()
    {
        EntityCommandBuffer entityCommandBuffer = new(Allocator.Temp);

        foreach (var (_, _, entity) in SystemAPI.Query<RefRW<UninitializedVehicleComponent>, RefRW<LocalTransform>>().WithEntityAccess())
        {
            VehicleComponent vehicleComponent = new()
            {
                seats = GetChildrenEntitiesWithComponent<VehicleSeatComponent>(entity),
                wheels = GetChildrenEntitiesWithComponent<WheelComponent>(entity)
            };

            entityCommandBuffer.AddComponent(entity, vehicleComponent);

            entityCommandBuffer.RemoveComponent<UninitializedVehicleComponent>(entity);
        }

        entityCommandBuffer.Playback(EntityManager);
        entityCommandBuffer.Dispose();
    }

    private FixedList128Bytes<Entity> GetChildrenEntitiesWithComponent<T>(Entity rootEntity) where T : unmanaged, IComponentData
    {
        DynamicBuffer<LinkedEntityGroup> childBuffer = EntityManager.GetBuffer<LinkedEntityGroup>(rootEntity);

        FixedList128Bytes<Entity> entitiesWithComponent = new();

        foreach (LinkedEntityGroup linkedEntityGroup in childBuffer)
        {
            if (!EntityManager.HasComponent<T>(linkedEntityGroup.Value)) continue;

            entitiesWithComponent.Add(linkedEntityGroup.Value);
        }

        return entitiesWithComponent;
    }
}