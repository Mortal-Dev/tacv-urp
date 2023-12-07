using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;
using System.Linq;
using Riptide;
using System.Diagnostics;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct NetworkEntitySyncSystem : ISystem
{
    private NetworkManagerEntityComponent networkManagerEntityComponent;

    public void OnUpdate(ref SystemState systemState)
    {
        if (!SystemAPI.TryGetSingleton(out networkManagerEntityComponent)) return;
        
        if (networkManagerEntityComponent.NetworkType == NetworkType.None) return;

        foreach (var (localTransformRecord, localTransform, networkedEntityComponent, entity) in SystemAPI.Query<RefRW<PreviousLocalTransformRecordComponent>, RefRO<LocalTransform>, RefRO<NetworkedEntityComponent>>().WithAll<LocalOwnedNetworkedEntityComponent>().WithEntityAccess())
        {
            bool updateLocalTransformOfParentEntity = false;

            if (!localTransformRecord.ValueRO.localTransformRecord.Equals(localTransform.ValueRO))
            {
                updateLocalTransformOfParentEntity = true;
                localTransformRecord.ValueRW.localTransformRecord = localTransform.ValueRO;
            }

            if (!SystemAPI.HasBuffer<Child>(entity))
            {
                SendSyncMessage(networkedEntityComponent.ValueRO.networkEntityId, localTransform.ValueRO, updateLocalTransformOfParentEntity, new List<NetworkedEntityChildrenMap>());
                continue;
            }

            DynamicBuffer<Child> children = systemState.EntityManager.GetBuffer<Child>(entity);

            List<NetworkedEntityChildrenMap> finalChangedChildrenMap = new();

            foreach (Child child in children)
            {
                List<NetworkedEntityChildrenMap> changedChildrenMaps = GetChangedChildrenPaths(child, ref systemState);

                foreach (NetworkedEntityChildrenMap changedChildMap in changedChildrenMaps) finalChangedChildrenMap.Add(changedChildMap);
            }

            SendSyncMessage(networkedEntityComponent.ValueRO.networkEntityId, localTransform.ValueRO, updateLocalTransformOfParentEntity, finalChangedChildrenMap);
        }
    }

    private List<NetworkedEntityChildrenMap> GetChangedChildrenPaths(Child root, ref SystemState systemState)
    {
        List<NetworkedEntityChildrenMap> result = new();

        FindChangedChildren(root, result, new NetworkedEntityChildrenMap(), ref systemState);

        return result;
    }

    private void FindChangedChildren(Child root, List<NetworkedEntityChildrenMap> result, NetworkedEntityChildrenMap current, ref SystemState systemState)
    {
        if (!SystemAPI.HasComponent<NetworkedEntityChildComponent>(root.Value)) return;

        NetworkedEntityChildComponent networkedEntityComponent = SystemAPI.GetComponent<NetworkedEntityChildComponent>(root.Value);
        PreviousLocalTransformRecordComponent previousLocalTransformRecordComponent = SystemAPI.GetComponent<PreviousLocalTransformRecordComponent>(root.Value);
        LocalTransform localTransform = SystemAPI.GetComponent<LocalTransform>(root.Value);

        current.networkedEntityChildList.Add(networkedEntityComponent.Id);
        
        if (!localTransform.Equals(previousLocalTransformRecordComponent.localTransformRecord))
        {
            previousLocalTransformRecordComponent.localTransformRecord = localTransform;
            SystemAPI.SetComponent(root.Value, previousLocalTransformRecordComponent);
            current.LocalTransform = localTransform;
            result.Add(current);
        }
        
        if (!SystemAPI.HasBuffer<Child>(root.Value)) return;

        DynamicBuffer<Child> children = SystemAPI.GetBuffer<Child>(root.Value);

        foreach (Child child in children)
        {
            FindChangedChildren(child, result, new NetworkedEntityChildrenMap() { networkedEntityChildList = current.networkedEntityChildList }, ref systemState );
        }
    }

    private readonly void SendSyncMessage(ulong parentNetworkedEntityId, LocalTransform parentNetworkedEntityTransform, bool updateNetworkedEntityTransform, List<NetworkedEntityChildrenMap> changedChildMap)
    {
        Message message = Message.Create(MessageSendMode.Unreliable, (networkManagerEntityComponent.NetworkType == NetworkType.Server || networkManagerEntityComponent.NetworkType == NetworkType.Host ? (ushort)ServerToClientNetworkMessageId.ServerSyncEntity : (ushort)ClientToServerNetworkMessageId.ClientSyncOwnedEntities));

        message.Add(parentNetworkedEntityId);

        message.Add(updateNetworkedEntityTransform);

        //child might update, but parent might not
        if (updateNetworkedEntityTransform) message.AddLocalTransform(parentNetworkedEntityTransform);

        message.Add(changedChildMap.Count);

        foreach (NetworkedEntityChildrenMap networkedEntityChildMapLocalTransform in changedChildMap)
        {
            message.AddInts(networkedEntityChildMapLocalTransform.networkedEntityChildList.ToArray());
            message.AddLocalTransform(networkedEntityChildMapLocalTransform.LocalTransform);
        }

        NetworkManager.Instance.Network.SendMessage(message, NetworkManager.Instance.NetworkType == NetworkType.Server || NetworkManager.Instance.NetworkType == NetworkType.Host ? SendMode.Server : SendMode.Client);
    }

}

class NetworkedEntityChildrenMap
{
    public List<int> networkedEntityChildList = new();

    public LocalTransform LocalTransform;
}