using Riptide;
using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine.SceneManagement;
using UnityEngine;

public class ServerNetworkedEntityContainer : NetworkedEntityContainer
{
    private IdGenerator networkIdGenerator;

    private EntityManager serverEntityManager;

    public ServerNetworkedEntityContainer(EntityManager serverEntityManager) : base()
    {
        networkIdGenerator = new IdGenerator();

        this.serverEntityManager = serverEntityManager;
    }

    public Entity GetNetworkedEntity(ulong networkId)
    {
        if (!NetworkedEntities.TryGetValue(networkId, out Entity value)) throw new Exception($"attempted to find a non-existent entity with the network id: {networkId}");

        return value;
    }

    public override ulong ActivateNetworkedEntity(Entity entity)
    {
        NetworkedEntityComponent networkedEntityComponent = serverEntityManager.GetComponentData<NetworkedEntityComponent>(entity);

        if (GetNetworkedPrefab(networkedEntityComponent.networkedPrefabHash) == Entity.Null) throw new Exception($"unable to find entity hash for unactivated entity with index: " + entity.Index);

        if (networkedEntityComponent.connectionId == NetworkManager.SERVER_NET_ID) serverEntityManager.AddComponent(entity, ComponentType.ReadWrite<LocalOwnedNetworkedEntityComponent>());

        ulong networkedEntityId = networkIdGenerator.GenerateId();

        networkedEntityComponent.networkEntityId = networkedEntityId;

        NetworkedEntities.Add(networkedEntityId, entity);

        SendSpawnNetworkedEntityMessage(networkedEntityComponent.networkedPrefabHash, networkedEntityComponent.connectionId, serverEntityManager.GetComponentData<LocalTransform>(entity), 
            networkedEntityId);

        serverEntityManager.SetComponentData(entity, networkedEntityComponent);

        return networkedEntityId;
    }

    public override ulong CreateNetworkedEntity(int networkedPrefabHash, ushort connectionOwnerId = NetworkManager.SERVER_NET_ID, ulong networkEntityId = ulong.MaxValue)
    {
        if (TryGetNetworkedPrefab(networkedPrefabHash, out Entity networkedPrefabEntity)) throw new Exception($"unable to find entity hash {networkedPrefabHash}");

        if (!serverEntityManager.HasComponent(networkedPrefabEntity, typeof(NetworkedEntityComponent))) throw new Exception("attempting to instantiate networked entity without a networked entity component");

        Entity entity = serverEntityManager.Instantiate(networkedPrefabEntity);

        ulong networkedEntityId = networkIdGenerator.GenerateId();

        if (connectionOwnerId == NetworkManager.SERVER_NET_ID) serverEntityManager.AddComponent(entity, ComponentType.ReadWrite<LocalOwnedNetworkedEntityComponent>());

        serverEntityManager.SetComponentData(entity, new NetworkedEntityComponent() { connectionId = connectionOwnerId, networkEntityId = networkedEntityId, networkedPrefabHash = networkedPrefabHash });

        NetworkedEntities.Add(networkedEntityId, entity);

        SendSpawnNetworkedEntityMessage(networkedPrefabHash, connectionOwnerId, serverEntityManager.GetComponentData<LocalTransform>(entity), networkedEntityId);

        return networkedEntityId;
    }

    public override void DestroyNetworkedEntity(ulong id)
    {
        if (!networkIdGenerator.IsIdInUse(id)) throw new Exception($"the networked id {id} was not found when attempting to destroy a networked entity");

        serverEntityManager.DestroyEntity(NetworkedEntities[id]);

        networkIdGenerator.DisposeId(id);

        NetworkedEntities.Remove(id);

        if (SceneEntitiesActive.ContainsKey(id)) SceneEntitiesActive[id] = false;

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

    private void SendSpawnNetworkedEntityMessage(int prefabHash, ushort connectionOwnerId, LocalTransform localTransform, ulong networkedEntityId, ushort sendToClientId = NetworkManager.SERVER_NET_ID)
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientNetworkMessageId.ServerSpawnEntity);
            
        message.Add(prefabHash);
        message.Add(connectionOwnerId);
        message.Add(networkedEntityId);
        message.AddLocalTransform(localTransform);

        NetworkManager.Instance.Network.SendMessage(message, SendMode.Server, sendToClientId);
    }

    private void SendDestroyNetworkedEntityMessage(ulong id)
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientNetworkMessageId.ServerDestroyEntity);

        message.AddULong(id);

        NetworkManager.Instance.Network.SendMessage(message, SendMode.Server);
    }

    [MessageHandler((ushort)ClientToServerNetworkMessageId.ClientSyncOwnedEntities)]
    private static void ServerRecieveSyncEntities(ushort clientId, Message message)
    {
        if (NetworkManager.CLIENT_NET_ID == clientId) return;

        NetworkSceneManager networkSceneManager = NetworkManager.Instance.NetworkSceneManager;

        ulong networkedEntityId = message.GetULong();

        Entity networkedEntity = networkSceneManager.NetworkedEntityContainer.GetEntity(networkedEntityId);

        bool updateParentEntity = message.GetBool();

        if (updateParentEntity) networkSceneManager.NetworkWorld.EntityManager.SetComponentData(networkedEntity, message.GetLocalTransform());

        int length = message.GetInt();

        for (int i = 0; i < length; i++)
        {
            Entity child = GetChildFromChildMap(networkedEntity, message.GetInts());
            networkSceneManager.NetworkWorld.EntityManager.SetComponentData(child, message.GetLocalTransform());
        }
    }

    private static Entity GetChildFromChildMap(Entity parent, int[] map)
    {
        EntityManager entityManager = NetworkManager.Instance.NetworkSceneManager.NetworkWorld.EntityManager;

        DynamicBuffer<Child> children = entityManager.GetBuffer<Child>(parent);

        DynamicBuffer<Child> newChildren = default;

        foreach (int childId in map)
        {
            if (newChildren.Length != 0) children = newChildren;

            foreach (Child child in children)
            {
                if (!entityManager.HasComponent<NetworkedEntityChildComponent>(child.Value)) continue;

                NetworkedEntityChildComponent networkedEntityChildComponent = entityManager.GetComponentData<NetworkedEntityChildComponent>(child.Value);

                if (networkedEntityChildComponent.Id != childId) continue;

                if (childId == map[^1]) return child.Value;

                newChildren = entityManager.GetBuffer<Child>(child.Value);

                break;
            }
        }

        throw new Exception("unable to find child entity");
    }
}