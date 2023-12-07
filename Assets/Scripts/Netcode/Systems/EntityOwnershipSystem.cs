using Riptide;
using Unity.Entities;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct EntityOwnershipSystem : ISystem
{
    public void OnUpdate(ref SystemState systemState)
    {
        if (NetworkManager.Instance.NetworkType == NetworkType.Client) return;

        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach (var (updateEntityOwnershipRequest, networkedEntityComponent, entity) in SystemAPI.Query<RefRO<UpdateEntityOwnershipComponent>, RefRO<NetworkedEntityComponent>>().WithEntityAccess())
        {
            entityCommandBuffer.RemoveComponent<UpdateEntityOwnershipComponent>(entity);

            Message changeVehicleOwnershipMessage = Message.Create(MessageSendMode.Reliable, ServerToClientNetworkMessageId.ServerChangeEntityOwnership);

            changeVehicleOwnershipMessage.Add(networkedEntityComponent.ValueRO.networkEntityId);
            changeVehicleOwnershipMessage.Add(updateEntityOwnershipRequest.ValueRO.newOwnerConnectionId);

            NetworkManager.Instance.Network.SendMessage(changeVehicleOwnershipMessage, SendMode.Server);
        }

        entityCommandBuffer.Playback(systemState.EntityManager);
        entityCommandBuffer.Dispose();
    }

    [MessageHandler((ushort)ServerToClientNetworkMessageId.ServerChangeEntityOwnership)]
    private static void ClientRecieveServerChangeEntityOwnership(Message message)
    {
        ulong networkId = message.GetULong();
        ushort newOwnerConnectionId = message.GetUShort();

        EntityManager entityManager = NetworkManager.Instance.NetworkSceneManager.NetworkWorld.EntityManager;

        Entity networkedEntity = NetworkManager.Instance.NetworkSceneManager.NetworkedEntityContainer.GetEntity(networkId);

        if (networkedEntity == Entity.Null)
        {
            UnityEngine.Debug.LogWarning($"attempted to change entity ownership for entity {networkId}, but it does not exist");
            return;
        }

        NetworkedEntityComponent networkedEntityComponent = entityManager.GetComponentData<NetworkedEntityComponent>(networkedEntity);

        if (networkedEntityComponent.connectionId == NetworkManager.CLIENT_NET_ID && newOwnerConnectionId != NetworkManager.CLIENT_NET_ID)
            entityManager.RemoveComponent<LocalOwnedNetworkedEntityComponent>(networkedEntity);

        networkedEntityComponent.connectionId = newOwnerConnectionId;

        entityManager.SetComponentData(networkedEntity, networkedEntityComponent);
    }
}