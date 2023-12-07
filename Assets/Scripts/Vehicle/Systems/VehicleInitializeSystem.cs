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
            List<Entity> vehicleWheels = GetChildrenEntitiesWithComponent<WheelComponent>(entity);
            List<Entity> vehicleSeats = GetChildrenEntitiesWithComponent<VehicleSeatComponent>(entity);

            SeatMapComponent seatMapComponent = SetSeats(vehicleSeats);

            entityCommandBuffer.RemoveComponent<UninitializedVehicleComponent>(entity);
            entityCommandBuffer.AddComponent(entity, seatMapComponent);
        }

        entityCommandBuffer.Playback(EntityManager);
        entityCommandBuffer.Dispose();
    }

    private List<Entity> GetChildrenEntitiesWithComponent<T>(Entity rootEntity) where T : unmanaged, IComponentData
    {
        DynamicBuffer<LinkedEntityGroup> childBuffer = EntityManager.GetBuffer<LinkedEntityGroup>(rootEntity);

        List<Entity> entitiesWithComponent = new();

        foreach (LinkedEntityGroup linkedEntityGroup in childBuffer)
        {
            if (!EntityManager.HasComponent<T>(linkedEntityGroup.Value)) continue;

            entitiesWithComponent.Add(linkedEntityGroup.Value);
        }

        return entitiesWithComponent;
    }

    private List<Entity> GetChildrenEntitiesWithComponentAndId<T>(Entity rootEntity) where T : unmanaged, ComponentId, IComponentData
    {
        List<Entity> entitiesWithComponent = GetChildrenEntitiesWithComponent<T>(rootEntity);

        entitiesWithComponent.OrderBy(entity => EntityManager.GetComponentData<T>(entity).Id);

        return entitiesWithComponent;
    }

    private SeatMapComponent SetSeats(List<Entity> seats)
    {
        SeatMapComponent seatMapComponent = new();

        seats.OrderBy(entity => EntityManager.GetComponentData<VehicleSeatComponent>(entity).seatPosition);

        foreach (Entity entity in seats)
        {
            seatMapComponent.seats.Add(entity);
        }

        return seatMapComponent;
    }
}