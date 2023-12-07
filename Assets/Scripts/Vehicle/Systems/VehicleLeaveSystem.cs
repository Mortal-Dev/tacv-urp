using Riptide;
using System;
using UnityEngine;
using System.Linq;
using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct VehicleLeaveSystem : ISystem
{
    public void OnUpdate(ref SystemState systemState)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach (var (vehicleLeaveComponent, playerEntity) in SystemAPI.Query<RefRO<RequestVehicleLeaveComponent>>().WithEntityAccess())
        {
            UnityEngine.Debug.Log("got leave request");

            Message message = Message.Create(MessageSendMode.Reliable, ClientToServerNetworkMessageId.ClientRequestVehicleLeave);

            message.Add(vehicleLeaveComponent.ValueRO.vehicleNetworkId);
            message.Add(vehicleLeaveComponent.ValueRO.seat);

            NetworkManager.Instance.Network.SendMessage(message, SendMode.Client);

            UnityEngine.Debug.Log("sent message");

            entityCommandBuffer.RemoveComponent<RequestVehicleLeaveComponent>(playerEntity);

            UnityEngine.Debug.Log("removed component");
        }

        if (!entityCommandBuffer.IsEmpty)
            UnityEngine.Debug.Log("playing back component removal");

        entityCommandBuffer.Playback(systemState.EntityManager);
        entityCommandBuffer.Dispose();
    }

    [MessageHandler((ushort)ClientToServerNetworkMessageId.ClientRequestVehicleLeave)]
    public static void ServerRecieveClientRequestVehicleLeave(ushort clientId, Message message)
    {
        ulong vehicleNetworkId = message.GetULong();
        int seatPosition = message.GetInt();

        Debug.Log("client id: " + clientId);

        Debug.Log("recieved message for client request vehicle leave");

        EntityManager entityManager = NetworkManager.Instance.NetworkSceneManager.NetworkWorld.EntityManager;

        Entity playerEntity = EntityHelper.GetEntityWithPredicate(entityManager, ComponentType.ReadWrite<PlayerComponent>(), x => entityManager.GetComponentData<NetworkedEntityComponent>(x).connectionId == clientId);

        if (playerEntity == Entity.Null)
        {
            Debug.LogWarning("unable to fine player entity");
            return;
        }

        var (seatEntity, seatComponent) = VehicleHelper.GetVehicleSeatEntity(vehicleNetworkId, seatPosition);

        var (vehicleEntity, vehicleComponent) = VehicleHelper.GetVehicleEntity(vehicleNetworkId);

        if (seatEntity == Entity.Null)
        {
            Debug.Log("unable to find seat entity");
        }

        if (vehicleEntity == Entity.Null)
        {
            Debug.Log("unablet of ind vehicle entity");
        }

        if (seatComponent.occupiedBy != playerEntity) return;

        seatComponent.occupiedBy = Entity.Null;

        entityManager.SetComponentData(seatEntity, seatComponent);

        Debug.Log("here1");

        if (vehicleComponent.currentSeatWithOwnership != seatEntity) return;

        vehicleComponent.currentSeatWithOwnership = Entity.Null;

        entityManager.SetComponentData(vehicleEntity, vehicleComponent);

        Debug.Log("here2");

        entityManager.AddComponentData(seatComponent.occupiedBy, new NetworkedUnparentRequestComponent() { rootParent = vehicleEntity });

        Debug.Log("here3");

        SendMessage();

        void SendMessage()
        {
            Message leaveMessage = Message.Create(MessageSendMode.Reliable, ServerToClientNetworkMessageId.ServerConfirmClientVehicleLeaveRequest);

            leaveMessage.Add(entityManager.GetComponentData<NetworkedEntityComponent>(vehicleEntity).networkEntityId);
            leaveMessage.Add(seatPosition);

            NetworkManager.Instance.Network.SendMessage(leaveMessage, SendMode.Server);
        }
    }

    [MessageHandler((ushort)ServerToClientNetworkMessageId.ServerConfirmClientVehicleLeaveRequest)]
    public static void ClientRecieveServerConfirmClientRequestVehicleLeave(Message message)
    {
        if (NetworkManager.Instance.NetworkType == NetworkType.Host) return;

        ulong vehicleNetworkId = message.GetULong();
        int seatPosition = message.GetInt();

        EntityManager entityManager = NetworkManager.Instance.NetworkSceneManager.NetworkWorld.EntityManager;

        var (vehicleEntity, vehicleComponent) = VehicleHelper.GetVehicleEntity(vehicleNetworkId);

        var (seatEntity, seatComponent) = VehicleHelper.GetVehicleSeatEntity(in vehicleComponent, seatPosition);

        seatComponent.occupiedBy = Entity.Null;

        entityManager.SetComponentData(seatEntity, seatComponent);

        if (vehicleComponent.currentSeatWithOwnership != seatEntity) return;

        vehicleComponent.currentSeatWithOwnership = Entity.Null;

        entityManager.SetComponentData(vehicleEntity, vehicleComponent);
    }
}