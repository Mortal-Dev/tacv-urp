using System;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;

public static class EntityHelper
{
    /*public static Entity GetPlayerEntity(EntityManager entityManager, ushort clientId)
    {
        EntityQuery entityQuery = entityManager.CreateEntityQuery(typeof(PlayerComponent));

        NativeArray<Entity> playerEntities = entityQuery.ToEntityArray(Allocator.Temp);

        Entity playerEntity = Array.Find(playerEntities.ToArray(), entity => entityManager.GetComponentData<NetworkedEntityComponent>(entity).connectionId == clientId);

        playerEntities.Dispose();

        return playerEntity;
    }*/

    public static Entity GetEntityWithPredicate(EntityManager entityManager, ComponentType componentRequest, Predicate<Entity> predicate)
    {
        EntityQuery entityQuery = entityManager.CreateEntityQuery(componentRequest);

        NativeArray<Entity> entityQueryResult = entityQuery.ToEntityArray(Allocator.Temp);

        Entity foundEntity = Entity.Null;

        foreach (Entity entity in entityQueryResult)
        {
            if (!predicate(entity)) continue;

            foundEntity = entity;

            break;
        }

        entityQueryResult.Dispose();
        return foundEntity;
    }
}