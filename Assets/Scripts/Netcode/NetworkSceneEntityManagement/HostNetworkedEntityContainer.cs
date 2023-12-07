using Riptide;
using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;

public class HostNetworkedEntityContainer : NetworkedEntityContainer
{
    private IdGenerator networkIdGenerator;

    private EntityManager hostEntityManager;

    private readonly Server server;

    public HostNetworkedEntityContainer(EntityManager hostEntityManager) : base()
    {
        networkIdGenerator = new IdGenerator();

        this.hostEntityManager = hostEntityManager;

        server = ((HostNetwork)NetworkManager.Instance.Network).Server;
    }

    public override ulong CreateNetworkedEntity(int networkedPrefabHash, ushort connectionOwnerId = ushort.MaxValue, ulong networkEntityId = ulong.MaxValue)
    {
        Entity entityPrefab = GetNetworkedPrefab(networkedPrefabHash);

        if (!hostEntityManager.HasComponent<NetworkedEntityComponent>(entityPrefab)) throw new Exception("attempting to instantiate networked entity without a networked entity component");

        Entity entity = hostEntityManager.Instantiate(entityPrefab);

        ulong networkId = networkIdGenerator.GenerateId();

        if (connectionOwnerId == NetworkManager.SERVER_NET_ID || connectionOwnerId == NetworkManager.CLIENT_NET_ID) 
            hostEntityManager.AddComponent(entity, ComponentType.ReadWrite<LocalOwnedNetworkedEntityComponent>());

        hostEntityManager.SetComponentData(entity, new NetworkedEntityComponent() { connectionId = connectionOwnerId, networkEntityId = networkId, networkedPrefabHash = networkedPrefabHash });

        NetworkedEntities.Add(networkId, entity);

        SendSpawnNetworkedEntityMessage(networkedPrefabHash, connectionOwnerId, hostEntityManager.GetComponentData<LocalTransform>(entity), networkId);

        return networkId;
    }

    public override ulong ActivateNetworkedEntity(Entity entity)
    {
        NetworkedEntityComponent networkedEntityComponent = hostEntityManager.GetComponentData<NetworkedEntityComponent>(entity);

        if (GetNetworkedPrefab(networkedEntityComponent.networkedPrefabHash) == Entity.Null) throw new Exception($"unable to find entity hash for unactivated entity with index: " + entity.Index);

        if (networkedEntityComponent.connectionId == NetworkManager.SERVER_NET_ID || networkedEntityComponent.connectionId == NetworkManager.CLIENT_NET_ID) 
            hostEntityManager.AddComponent(entity, ComponentType.ReadWrite<LocalOwnedNetworkedEntityComponent>());

        ulong networkId = networkIdGenerator.GenerateId();

        networkedEntityComponent.networkEntityId = networkId;

        NetworkedEntities.Add(networkId, entity);

        SendSpawnNetworkedEntityMessage(networkedEntityComponent.networkedPrefabHash, networkedEntityComponent.connectionId, hostEntityManager.GetComponentData<LocalTransform>(entity), networkId);

        hostEntityManager.SetComponentData(entity, networkedEntityComponent);

        return networkId;
    }

    public override void DestroyNetworkedEntity(ulong id)
    {
        if (!networkIdGenerator.IsIdInUse(id)) throw new Exception($"the networked id {id} was not found when attempting to destroy a networked entity");

        hostEntityManager.DestroyEntity(NetworkedEntities[id]);

        NetworkedEntities.Remove(id);

        networkIdGenerator.DisposeId(id);

        SendDestroyNetworkedEntityMessage(id);
    }

    public override void DestroyAllNetworkedEntities()
    {
        foreach (KeyValuePair<ulong, Entity> idEntityPair in NetworkedEntities)
        {
            DestroyNetworkedEntity(idEntityPair.Key);
        }

        networkIdGenerator = new IdGenerator();
        NetworkedEntities.Clear();
    }

    private void SendSpawnNetworkedEntityMessage(int prefabHash, ushort connectionOwnerId, LocalTransform localTransform, ulong networkId, ushort sendToClientId = NetworkManager.SERVER_NET_ID)
    {
        Message message;

        //don't send spawn to ourself
        if (sendToClientId == NetworkManager.CLIENT_NET_ID) return;

        //send spawn if it's an individual, not everyone
        if (sendToClientId != NetworkManager.SERVER_NET_ID)
        {
            NetworkManager.Instance.Network.SendMessage(CreateSpawnedNetworkedEntityMessage(), SendMode.Server, sendToClientId);
        }
        else
        {  //send spawn to everyone except ourself
            foreach (Connection connection in server.Clients)
            {
                if (connection.Id == NetworkManager.CLIENT_NET_ID) continue;

                NetworkManager.Instance.Network.SendMessage(CreateSpawnedNetworkedEntityMessage(), SendMode.Server, connection.Id);
            }
        }

        Message CreateSpawnedNetworkedEntityMessage()
        {
            message = Message.Create(MessageSendMode.Reliable, ServerToClientNetworkMessageId.ServerSpawnEntity);

            message.Add(prefabHash);
            message.Add(connectionOwnerId);
            message.Add(networkId);
            message.AddLocalTransform(localTransform);

            return message;
        }

    }

    private void SendDestroyNetworkedEntityMessage(ulong id)
    {
        foreach (Connection connection in server.Clients)
        {
            if (connection.Id == NetworkManager.CLIENT_NET_ID) continue;

            NetworkManager.Instance.Network.SendMessage(Message.Create(MessageSendMode.Reliable, ServerToClientNetworkMessageId.ServerDestroyEntity).AddULong(id), SendMode.Server, connection.Id);
        }
    }
}