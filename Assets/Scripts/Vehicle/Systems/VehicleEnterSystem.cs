using Riptide;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using System.Linq;
using System;
using Unity.Physics;
using UnityEngine;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct VehicleEnterSystem : ISystem
{
    public void OnUpdate(ref SystemState systemState)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (requestVehicleEnterComponent, entity) in SystemAPI.Query<RefRO<RequestVehicleEnterComponent>>().WithEntityAccess())
        {
            Message message = Message.Create(MessageSendMode.Reliable, ClientToServerNetworkMessageId.ClientRequestVehicleEnter);

            message.Add(requestVehicleEnterComponent.ValueRO.vehicleNetworkId);
            message.Add(requestVehicleEnterComponent.ValueRO.seat);

            NetworkManager.Instance.Network.SendMessage(message, SendMode.Client);

            entityCommandBuffer.RemoveComponent(entity, ComponentType.ReadWrite<RequestVehicleEnterComponent>());
        }

        entityCommandBuffer.Playback(systemState.EntityManager);
        entityCommandBuffer.Dispose();
    }

    [MessageHandler((ushort)ClientToServerNetworkMessageId.ClientRequestVehicleEnter)]
    public static void ServerRecieveClientRequestVehicleEnter(ushort clientId, Message message)
    {
        ulong vehicleNetworkId = message.GetULong();
        int seatPosition = message.GetInt();

        EntityManager entityManager = NetworkManager.Instance.NetworkSceneManager.NetworkWorld.EntityManager;

        Entity playerEntity = EntityHelper.GetEntityWithPredicate(entityManager, ComponentType.ReadWrite<PlayerComponent>(), entity => entityManager.GetComponentData<NetworkedEntityComponent>(entity).connectionId == clientId);

        if (entityManager.HasComponent<InVehicleComponent>(playerEntity)) return;

        var (vehicleEntity, vehicleComponent) = VehicleHelper.GetVehicleEntity(vehicleNetworkId);

        var (seatEntity, vehicleSeatComponent) = VehicleHelper.GetVehicleSeatEntity(vehicleNetworkId, seatPosition);

        if (seatEntity == Entity.Null)
        {
            Debug.Log($"unable to find seat position {seatPosition} in client seat request");
            return;
        }

        if (vehicleSeatComponent.isOccupied) return;

        vehicleSeatComponent.isOccupied = true;

        vehicleSeatComponent.occupiedBy = playerEntity;

        SendConfirmVehicleEnterMessage();

        if (vehicleComponent.currentSeatWithOwnership == null && vehicleSeatComponent.hasOwnershipCapability)
        {
            vehicleComponent.currentSeatWithOwnership = seatEntity;
            vehicleSeatComponent.hasOwnership = true;

            entityManager.AddComponentData(playerEntity, new UpdateEntityOwnershipComponent() { newOwnerConnectionId = NetworkManager.SERVER_NET_ID });
        }

        SetComponents();

        void SendConfirmVehicleEnterMessage()
        {
            Message confirmClientVehicleEnterRequest = Message.Create(MessageSendMode.Reliable, ServerToClientNetworkMessageId.ServerConfirmClientVehicleEnterRequest);

            confirmClientVehicleEnterRequest.Add(clientId);
            confirmClientVehicleEnterRequest.Add(vehicleNetworkId);
            confirmClientVehicleEnterRequest.Add(vehicleSeatComponent.seatPosition);

            NetworkManager.Instance.Network.SendMessage(confirmClientVehicleEnterRequest, SendMode.Server);
        }

        void SetComponents()
        {
            entityManager.SetComponentData(seatEntity, vehicleSeatComponent);

            entityManager.SetComponentData(vehicleEntity, vehicleComponent);

            entityManager.AddComponentData(playerEntity, new InVehicleComponent() { seat = seatEntity, vehicle = vehicleEntity });

            entityManager.AddComponentData(playerEntity, new NetworkedParentRequestComponent() { rootNewParent = vehicleEntity, newParentChildId = entityManager.GetComponentData<NetworkedEntityChildComponent>(seatEntity).Id });
        }
    }

    [MessageHandler((ushort)ServerToClientNetworkMessageId.ServerConfirmClientVehicleEnterRequest)]
    public static void ClientRecieveServerConfirmClientRequestVehicleEnter(Message message)
    {
        int clientIdEnteringVehicle = message.GetUShort();
        ulong vehicleNetworkId = message.GetULong();
        int seatPosition = message.GetInt();

        EntityManager entityManager = NetworkManager.Instance.NetworkSceneManager.NetworkWorld.EntityManager;

        var (vehicleEntity, vehicleComponent) = VehicleHelper.GetVehicleEntity(vehicleNetworkId);

        var (vehicleSeatEntity, vehicleSeatComponent) = VehicleHelper.GetVehicleSeatEntity(vehicleNetworkId, seatPosition);

        Entity playerEntity = EntityHelper.GetEntityWithPredicate(entityManager, ComponentType.ReadWrite<PlayerComponent>(), x => entityManager.GetComponentData<NetworkedEntityComponent>(x).connectionId == clientIdEnteringVehicle);

        if (NetworkManager.Instance.NetworkType != NetworkType.Host) entityManager.AddComponentData(playerEntity, new InVehicleComponent() { seat = vehicleSeatEntity, vehicle = vehicleSeatEntity });

        vehicleSeatComponent.occupiedBy = playerEntity;

        if (vehicleComponent.currentSeatWithOwnership == Entity.Null && vehicleSeatComponent.hasOwnershipCapability)
        {
            vehicleComponent.currentSeatWithOwnership = vehicleSeatEntity;
            vehicleSeatComponent.hasOwnership = true;

            entityManager.SetComponentData(vehicleEntity, vehicleComponent);
        }

        entityManager.SetComponentData(vehicleSeatEntity, vehicleSeatComponent);
    }
}