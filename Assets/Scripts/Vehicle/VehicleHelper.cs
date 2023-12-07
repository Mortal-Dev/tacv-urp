using System;
using Unity.Entities;

public static class VehicleHelper
{
    public static (Entity, VehicleComponent) GetVehicleEntity(ulong vehicleNetworkId)
    {
        EntityManager entityManager = NetworkManager.Instance.NetworkSceneManager.NetworkWorld.EntityManager;

        Entity vehiclEntity = EntityHelper.GetEntityWithPredicate(entityManager, ComponentType.ReadWrite<VehicleComponent>(), x => entityManager.GetComponentData<NetworkedEntityComponent>(x).networkEntityId == vehicleNetworkId);

        VehicleComponent vehicleComponent = entityManager.GetComponentData<VehicleComponent>(vehiclEntity);

        return (vehiclEntity, vehicleComponent);
    }

    public static (Entity, VehicleSeatComponent) GetVehicleSeatEntity(ulong vehicleNetworkId, int seatPosition)
    {
        EntityManager entityManager = NetworkManager.Instance.NetworkSceneManager.NetworkWorld.EntityManager;

        Entity vehicleEntity = EntityHelper.GetEntityWithPredicate(entityManager, ComponentType.ReadWrite<VehicleComponent>(), x => entityManager.GetComponentData<NetworkedEntityComponent>(x).networkEntityId == vehicleNetworkId);

        VehicleComponent vehicleComponent = entityManager.GetComponentData<VehicleComponent>(vehicleEntity);

        return GetVehicleSeatEntity(in vehicleComponent, seatPosition);
    }

    public static (Entity, VehicleSeatComponent) GetVehicleSeatEntity(in VehicleComponent vehicleComponent, int seatPosition)
    {
        EntityManager entityManager = NetworkManager.Instance.NetworkSceneManager.NetworkWorld.EntityManager;

        foreach (Entity seatEntity in vehicleComponent.seats)
        {
            VehicleSeatComponent vehicleSeatComponent = entityManager.GetComponentData<VehicleSeatComponent>(seatEntity);

            if (vehicleSeatComponent.seatPosition == seatPosition)
                return (seatEntity, vehicleSeatComponent);
        }

        return (Entity.Null, default);
    }
}