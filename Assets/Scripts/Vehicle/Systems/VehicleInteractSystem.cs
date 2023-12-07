using UnityEngine;
using Unity.Burst;
using Unity.Entities;
using Unity.Collections;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(VehicleEnterSystem))]
[UpdateBefore(typeof(VehicleLeaveSystem))]
[BurstCompile]
public partial struct VehicleInteractSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState systemState)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (playerComponent, inputComponent, localOwnedNetworkedEntityComponent, playerEntity) in SystemAPI.Query<RefRO<PlayerComponent>, RefRO<PlayerControllerInputComponent>, RefRO<LocalOwnedNetworkedEntityComponent>>()
            .WithNone<RequestVehicleEnterComponent>().WithNone<RequestVehicleLeaveComponent>().WithEntityAccess())
        {
            if (inputComponent.ValueRO.rightControllerButtonOne > 0)
            {
                if (systemState.EntityManager.HasComponent<InVehicleComponent>(playerEntity)) continue;

                foreach (RefRO<NetworkedEntityComponent> networkedEntityComponent in SystemAPI.Query<RefRO<NetworkedEntityComponent>>().WithAll<VehicleComponent>())
                {
                    entityCommandBuffer.AddComponent(playerEntity, new RequestVehicleEnterComponent() { seat = 0, vehicleNetworkId = networkedEntityComponent.ValueRO.networkEntityId });
                }
            }
            else if (inputComponent.ValueRO.rightControllerButtonTwo > 0)
            {
                Debug.Log($"{"exit vehicle called"}");

                if (!systemState.EntityManager.HasComponent<InVehicleComponent>(playerEntity)) continue;

                entityCommandBuffer.AddComponent(playerEntity, new RequestVehicleLeaveComponent() { seat = 0, vehicleNetworkId = 2009964156 });

                Debug.Log($"{"added leave request"}");

                continue;
            }
        }

        entityCommandBuffer.Playback(systemState.EntityManager);
        entityCommandBuffer.Dispose();
    }
}