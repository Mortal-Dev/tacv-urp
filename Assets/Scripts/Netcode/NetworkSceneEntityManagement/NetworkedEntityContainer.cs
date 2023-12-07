using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public abstract class NetworkedEntityContainer
{
    public readonly Dictionary<ulong, Entity> NetworkedEntities;

    public readonly Dictionary<ulong, bool> SceneEntitiesActive;

    public NetworkedEntityContainer()
    {
        NetworkedEntities = new Dictionary<ulong, Entity>();

        SceneEntitiesActive = new Dictionary<ulong, bool>();
    }

    public IEnumerator<KeyValuePair<ulong, Entity>> GetEnumerator()
    {
        return NetworkedEntities.GetEnumerator();
    }

    public Entity GetEntity(ulong id)
    {
        return NetworkedEntities[id];
    }

    public Entity GetChildNetworkedEntity(ulong parentId, int childId)
    {
        if (!NetworkedEntities.TryGetValue(parentId, out Entity networkedEntityParent)) return Entity.Null;

        EntityManager entityManager = NetworkManager.Instance.NetworkSceneManager.NetworkWorld.EntityManager;

        DynamicBuffer<LinkedEntityGroup> linkedEntityGroupChildBuffer = entityManager.GetBuffer<LinkedEntityGroup>(networkedEntityParent);

        foreach (LinkedEntityGroup linkedEntityGroup in linkedEntityGroupChildBuffer)
        {
            if (!entityManager.HasComponent<NetworkedEntityChildComponent>(linkedEntityGroup.Value))
                continue;

            NetworkedEntityChildComponent networkedEntityChildComponent = entityManager.GetComponentData<NetworkedEntityChildComponent>(linkedEntityGroup.Value);

            if (networkedEntityChildComponent.Id == childId)
                return linkedEntityGroup.Value;
        }

        return Entity.Null;
    }

    public abstract ulong CreateNetworkedEntity(int networkedPrefabHash, ushort connectionOwnerId = NetworkManager.SERVER_NET_ID, ulong networkEntityId = ulong.MaxValue);

    public abstract ulong ActivateNetworkedEntity(Entity entity);

    public ulong CreateNetworkedEntity(Entity networkedEntityPrefab, ushort connectionOwnerId = NetworkManager.SERVER_NET_ID, ulong networkEntityId = ulong.MaxValue)
    {
        NetworkedEntityComponent networkedEntityComponent = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<NetworkedEntityComponent>(networkedEntityPrefab);

        return CreateNetworkedEntity(networkedEntityComponent.networkedPrefabHash, connectionOwnerId, networkEntityId);
    }

    public abstract void DestroyNetworkedEntity(ulong id);

    public abstract void DestroyAllNetworkedEntities();

    public void SetupSceneNetworkedEntities()
    {
        Debug.Log("begin scene networked entities setup");

        SceneEntitiesActive.Clear();

        EntityManager entityManager = NetworkManager.Instance.NetworkSceneManager.NetworkWorld.EntityManager;

        EntityQuery entityQuery = entityManager.CreateEntityQuery(typeof(NetworkedEntityComponent));

        NativeArray<Entity> networkedEntities = entityQuery.ToEntityArray(Allocator.Temp);

        foreach (Entity entity in networkedEntities)
        {
            NetworkedEntityComponent networkedEntityComponent = entityManager.GetComponentData<NetworkedEntityComponent>(entity);
            SceneEntitiesActive.Add(networkedEntityComponent.networkEntityId, true);
            ActivateNetworkedEntity(entity);
        }

        networkedEntities.Dispose();

        Debug.Log("finished scene networked entities setup");
    }

    public Entity GetNetworkedPrefab(int networkedPrefabHash)
    {
        EntityManager entityManager = NetworkManager.Instance.NetworkSceneManager.NetworkWorld.EntityManager;

        EntityQuery entityQuery = entityManager.CreateEntityQuery(typeof(NetworkedPrefabComponent));

        NativeArray<NetworkedPrefabComponent> prefabs = entityQuery.ToComponentDataArray<NetworkedPrefabComponent>(Allocator.Temp);

        foreach (NetworkedPrefabComponent networkedPrefabComponent in prefabs)
        {
            NetworkedEntityComponent networkedEntityComponent = entityManager.GetComponentData<NetworkedEntityComponent>(networkedPrefabComponent.prefab);

            if (networkedEntityComponent.networkedPrefabHash == networkedPrefabHash) return networkedPrefabComponent.prefab;
        }

        UnityEngine.Debug.LogError("unable to find hash: " + networkedPrefabHash);
        return Entity.Null;
    }

    public bool TryGetNetworkedPrefab(int networkedPrefabHash, out Entity networkedPrefab)
    {
        networkedPrefab = Entity.Null;

        EntityManager entityManager = NetworkManager.Instance.NetworkSceneManager.NetworkWorld.EntityManager;

        EntityQuery entityQuery = entityManager.CreateEntityQuery(typeof(NetworkedPrefabComponent));

        NativeArray<NetworkedPrefabComponent> prefabs = entityQuery.ToComponentDataArray<NetworkedPrefabComponent>(Allocator.Temp);

        foreach (NetworkedPrefabComponent networkedPrefabComponent in prefabs)
        {
            NetworkedEntityComponent networkedEntityComponent = entityManager.GetComponentData<NetworkedEntityComponent>(networkedPrefabComponent.prefab);

            if (networkedEntityComponent.networkedPrefabHash == networkedPrefabHash)
            {
                networkedPrefab = networkedPrefabComponent.prefab;
                return true;
            }
        }

        return false;
    }
}