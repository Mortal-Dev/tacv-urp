using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using Riptide;
using UnityEngine.SceneManagement;
using System.Diagnostics;

public class ClientNetworkedEntityContainer : NetworkedEntityContainer
{
    private EntityManager clientEntityManager;

    public ClientNetworkedEntityContainer(EntityManager clientEntityManager)
    {
        this.clientEntityManager = clientEntityManager;
    }

    public Entity GetNetworkedEntity(uint networkId)
    {
        if (!NetworkedEntities.TryGetValue(networkId, out Entity value)) throw new Exception($"attempted to find a non-existent entity with the network id: {networkId}");

        return value;
    }

    public override ulong CreateNetworkedEntity(int networkedPrefabHash, ushort connectionOwnerId = NetworkManager.SERVER_NET_ID, ulong networkEntityId = ulong.MaxValue)
    {
        Entity entityPrefab = GetNetworkedPrefab(networkedPrefabHash);

        if (entityPrefab == Entity.Null) throw new Exception($"unable to find entity hash for prefab entity with hash: " + networkedPrefabHash);

        Entity spawnedNetworkedEntity = clientEntityManager.Instantiate(entityPrefab);

        clientEntityManager.SetComponentData(spawnedNetworkedEntity, new NetworkedEntityComponent() { networkEntityId = networkEntityId, connectionId = connectionOwnerId, networkedPrefabHash = networkedPrefabHash });

        if (connectionOwnerId == NetworkManager.CLIENT_NET_ID) clientEntityManager.AddComponent(spawnedNetworkedEntity, ComponentType.ReadWrite(typeof(LocalOwnedNetworkedEntityComponent)));
        
        NetworkedEntities.Add(networkEntityId, spawnedNetworkedEntity);

        return networkEntityId;
    }

    public override void DestroyNetworkedEntity(ulong networkId)
    {
        if (!NetworkedEntities.TryGetValue(networkId, out Entity networkedEntity)) throw new Exception($"attempted to destroy an entity with the id {networkId} that doesn't exist");

        clientEntityManager.DestroyEntity(networkedEntity);

        NetworkedEntities.Remove(networkId);
    }

    public override void DestroyAllNetworkedEntities()
    {
        foreach (KeyValuePair<ulong, Entity> idEntityPair in NetworkedEntities)
        {
            DestroyNetworkedEntity(idEntityPair.Key);
        }

        NetworkedEntities.Clear();
    }

    [MessageHandler((ushort)ServerToClientNetworkMessageId.ServerSpawnEntity)]
    private static void SpawnNetworkedEntityRecieved(Message message)
    {
        int networkedPrefabHash = message.GetInt();
        ushort ownerId = message.GetUShort();
        ulong networkId = message.GetULong();
        LocalTransform localTransform = message.GetLocalTransform();

        NetworkManager.Instance.NetworkSceneManager.NetworkedEntityContainer.CreateNetworkedEntity(networkedPrefabHash, ownerId, networkId);
        Entity networkEntity = NetworkManager.Instance.NetworkSceneManager.NetworkedEntityContainer.GetEntity(networkId);

        NetworkManager.Instance.NetworkSceneManager.NetworkWorld.EntityManager.SetComponentData(networkEntity, localTransform);
    }

    [MessageHandler((ushort)ServerToClientNetworkMessageId.ServerDestroyEntity)]
    private static void DestroyNetworkedEntityRecieved(Message message)
    {
        ulong networkedEntityId = message.GetULong();

        NetworkManager.Instance.NetworkSceneManager.NetworkWorld.EntityManager.DestroyEntity(NetworkManager.Instance.NetworkSceneManager.NetworkedEntityContainer.GetEntity(networkedEntityId));
    }

    [MessageHandler((ushort)ServerToClientNetworkMessageId.ServerSyncEntity)]
    private static void ClientRecieveSyncEntities(Message message)
    {
        if (NetworkManager.Instance.NetworkType == NetworkType.Host) return;

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

    [MessageHandler((ushort)ServerToClientNetworkMessageId.ServerDestroyDefaultSceneEntity)]
    private static void ClientRecieveServerDestroyDefaultSceneEntity(Message message)
    {
        ulong networkId = message.GetULong();

        NetworkManager.Instance.NetworkSceneManager.NetworkedEntityContainer.DestroyNetworkedEntity(networkId);
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

    public override ulong ActivateNetworkedEntity(Entity entity)
    {
        throw new Exception("cannot active entites from client, must be server");
    }
}