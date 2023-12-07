using Unity.Burst;
using Unity.Entities;
using Riptide;
using Unity.Collections;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PlayerInputSystem))]
public partial struct FixedWingInputSystem : ISystem
{
    public void OnUpdate(ref SystemState systemState)
    {
        if (!SystemAPI.TryGetSingleton(out PlayerControllerInputComponent playerControllerInputComponent)) return;

        foreach (var (fixedWingInputComponent, localPlayerInVehicleComponent, vehicleNetworkedEntityComponent) in SystemAPI.Query<RefRW<FixedWingInputComponent>, RefRO<InVehicleComponent>, 
            RefRO<NetworkedEntityComponent>>())
        {
            switch (localPlayerInVehicleComponent.ValueRO.ownershipType)
            {
                case OwnershipType.Owned:
                    fixedWingInputComponent.ValueRW.stickInput = playerControllerInputComponent.rightControllerThumbstick;
                    fixedWingInputComponent.ValueRW.throttle = 1f;
                    break;

               /* case OwnershipType.Shared:
                    if (localPlayerInVehicleComponent.ValueRO.currentOwnerId == -1)
                    {
                        Message message = Message.Create(MessageSendMode.Reliable, NetworkMessageId.ClientRequestVehicleOwnership);
                        message.Add(vehicleNetworkedEntityComponent.ValueRO.networkEntityId);
                        NetworkManager.Instance.Network.SendMessage(message, SendMode.Client);
                    }
                    break;*/
            }
        }
    }

    [MessageHandler((ushort)ClientToServerNetworkMessageId.ClientRequestVehicleControl)]
    public static void OwnerRequestChangeRecieved(ushort clientId, Message message)
    {
        ulong vehicleNetworkId = message.GetULong();

        EntityManager entityManager = NetworkManager.Instance.NetworkSceneManager.NetworkWorld.EntityManager;

        Entity vehicleEntity = NetworkManager.Instance.NetworkSceneManager.NetworkedEntityContainer.GetEntity(vehicleNetworkId);

        VehicleComponent vehicleComponent = entityManager.GetComponentData<VehicleComponent>(vehicleEntity);


    }
}
