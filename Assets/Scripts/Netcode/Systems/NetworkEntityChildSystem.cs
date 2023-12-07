using Riptide;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(NetworkEntitySyncSystem))]
public partial struct NetworkEntityChildSystem : ISystem
{
    public void OnUpdate(ref SystemState systemState)
    {
        if (!SystemAPI.TryGetSingleton(out NetworkManagerEntityComponent networkManagerEntityComponent)) return;

        if (networkManagerEntityComponent.NetworkType != NetworkType.Host || networkManagerEntityComponent.NetworkType == NetworkType.Server) return;

        EntityCommandBuffer entityCommandBuffer = new(Unity.Collections.Allocator.Temp);

        foreach (var (networkedParentRequest, entity) in SystemAPI.Query<NetworkedParentRequestComponent>().WithEntityAccess())
        {
            Debug.Log("network unparent request");

            entityCommandBuffer.RemoveComponent<NetworkedParentRequestComponent>(entity);

            NetworkedEntityComponent networkedEntityParentComponent = SystemAPI.GetComponent<NetworkedEntityComponent>(networkedParentRequest.rootNewParent);

            NetworkedEntityComponent networkedEntityChildComponent = SystemAPI.GetComponent<NetworkedEntityComponent>(entity);

            Entity networkedEntityParent = NetworkManager.Instance.NetworkSceneManager.NetworkedEntityContainer.GetEntity(networkedEntityParentComponent.networkEntityId);

            Entity networkedEntityChild = NetworkManager.Instance.NetworkSceneManager.NetworkedEntityContainer.GetEntity(networkedEntityChildComponent.networkEntityId);

            Entity childOfParentNetworkedEntity = networkedParentRequest.newParentChildId == 0 ? networkedEntityParent : NetworkManager.Instance.NetworkSceneManager.NetworkedEntityContainer.GetChildNetworkedEntity(networkedEntityParentComponent.networkEntityId, networkedParentRequest.newParentChildId);

            Debug.Log("check parent entity");

            if (!CheckParentEntity(networkedEntityParent, in networkedEntityParentComponent)) continue;

            Debug.Log("setting components and message");

            RemovePhysicsComponents(networkedEntityChild, entityCommandBuffer, ref systemState);

            SetNetworkedParent(childOfParentNetworkedEntity, networkedEntityChild, entityCommandBuffer);

            SendNewParentMessage(in networkedEntityChildComponent, in networkedEntityParentComponent, in networkedParentRequest);
        }

        foreach (var (_, entity) in SystemAPI.Query<NetworkedUnparentRequestComponent>().WithEntityAccess())
        {
            Debug.Log("unparent request");

            entityCommandBuffer.RemoveComponent<Parent>(entity);
            entityCommandBuffer.RemoveComponent<ChildedNetworkedEntityComponent>(entity);
            entityCommandBuffer.RemoveComponent<NetworkedUnparentRequestComponent>(entity);

            SendUnparentMessage(SystemAPI.GetComponent<NetworkedEntityComponent>(entity).networkEntityId);
        }

        entityCommandBuffer.Playback(systemState.EntityManager);
        entityCommandBuffer.Dispose();
    }

    private readonly void SendNewParentMessage(in NetworkedEntityComponent networkedEntityChildComponent, in NetworkedEntityComponent networkedEntityParentComponent,
        in NetworkedParentRequestComponent networkedParentRequest)
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientNetworkMessageId.ServerSetNetworkEntityParent);
        message.Add(networkedEntityChildComponent.networkEntityId);
        message.Add(networkedEntityParentComponent.networkEntityId);
        message.Add(networkedParentRequest.newParentChildId);
        NetworkManager.Instance.Network.SendMessage(message, SendMode.Server);
    }

    private readonly void RemovePhysicsComponents(Entity entity, EntityCommandBuffer entityCommandBuffer, ref SystemState systemState)
    {
        // entityCommandBuffer.SetComponent(entity, new PhysicsVelocity { Linear = float3.zero, Angular = float3.zero });

        // StoredChildedNetworkEntityCollider storedChildedNetworkEntityCollider = new() { physicsCollider = SystemAPI.GetComponent<PhysicsCollider>(entity) };

        //  entityCommandBuffer.AddComponent(entity, storedChildedNetworkEntityCollider);
        entityCommandBuffer.RemoveComponent<PhysicsVelocity>(entity);
        entityCommandBuffer.RemoveComponent<PhysicsCollider>(entity);
        entityCommandBuffer.RemoveComponent<PhysicsMass>(entity);
    }

    private readonly bool CheckParentEntity(Entity networkedEntityParent, in NetworkedEntityComponent parentNetworkedEntityComponent)
    {
        if (networkedEntityParent == Entity.Null)
        {
            Debug.LogError($"attempted to create parent request to a non existent networked entity {networkedEntityParent}");
            return false;
        }

        if (!parentNetworkedEntityComponent.allowNetworkedChildrenRequests)
        {
            Debug.LogWarning($"attempted to parent to {parentNetworkedEntityComponent.networkEntityId}, but it is not allowed to have children");
            return false;
        }

        return true;
    }

    private readonly void SendUnparentMessage(ulong childNetworkId)
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientNetworkMessageId.ServerUnparentNetworkEntity);

        message.Add(childNetworkId);

        NetworkManager.Instance.Network.SendMessage(message, SendMode.Server);
    }

    private static void SetNetworkedParent(Entity parent, Entity child, EntityCommandBuffer entityCommandBuffer)
    {
        entityCommandBuffer.AddComponent(child, new Parent { Value = parent });
        entityCommandBuffer.SetComponent(child, new LocalTransform { Position = float3.zero, Rotation = quaternion.identity, Scale = 1 });
        
        entityCommandBuffer.AddComponent(child, new ChildedNetworkedEntityComponent());
    }

    [MessageHandler((ushort)ServerToClientNetworkMessageId.ServerSetNetworkEntityParent)]
    public static void ServerSetNetworkedParent(Message message)
    {
        if (NetworkManager.Instance.NetworkType == NetworkType.Host) return;

        ulong childNetworkedId = message.GetULong();
        ulong parentNetworkedId = message.GetULong();
        int childId = message.GetInt();

        Entity childNetworkedEntity = NetworkManager.Instance.NetworkSceneManager.NetworkedEntityContainer.GetEntity(childNetworkedId);
        Entity parentNetworkedEntity = NetworkManager.Instance.NetworkSceneManager.NetworkedEntityContainer.GetEntity(parentNetworkedId);
        Entity childOfParentNetworkedEntity = childId == 0 ? parentNetworkedEntity : NetworkManager.Instance.NetworkSceneManager.NetworkedEntityContainer.GetChildNetworkedEntity(parentNetworkedId, childId);

        EntityCommandBuffer entityCommandBuffer = new(Unity.Collections.Allocator.Temp);

        SetNetworkedParent(childNetworkedEntity, childOfParentNetworkedEntity, entityCommandBuffer);

        entityCommandBuffer.Playback(NetworkManager.Instance.NetworkSceneManager.NetworkWorld.EntityManager);
        entityCommandBuffer.Dispose();
    }

    [MessageHandler((ushort)ServerToClientNetworkMessageId.ServerUnparentNetworkEntity)]
    public static void ServerUnparentNetworkedEntity(Message message)
    {
        EntityManager entityManager = NetworkManager.Instance.NetworkSceneManager.NetworkWorld.EntityManager;

        ulong childNetworkedId = message.GetULong();

        Entity childNetworkedEntity = NetworkManager.Instance.NetworkSceneManager.NetworkedEntityContainer.GetEntity(childNetworkedId);

        entityManager.RemoveComponent<Parent>(childNetworkedEntity);
        entityManager.RemoveComponent<ChildedNetworkedEntityComponent>(childNetworkedEntity);
    }
}