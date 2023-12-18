using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public static class LocalTransformExtensions
{
    public static LocalTransform ConvertLocalEntityTransformToGlobalTransform(this LocalToWorld entityLocalToWorld, Entity localEntity, EntityManager entityManager)
    {
        float3 newPosition = entityLocalToWorld.Position;
        Quaternion newRotation = entityLocalToWorld.Rotation;

        bool hasParent = entityManager.HasComponent<Parent>(localEntity);

        while (hasParent)
        {
            Entity parent = entityManager.GetComponentData<Parent>(localEntity).Value;

            LocalToWorld childLocalToWorld = entityManager.GetComponentData<LocalToWorld>(localEntity);

            LocalTransform childTransform = new() { Position = childLocalToWorld.Position, Rotation = childLocalToWorld.Rotation, Scale = 1 };

            if (entityManager.HasComponent<Parent>(localEntity))
            {
                Entity nextRootEntity = entityManager.GetComponentData<Parent>(localEntity).Value;

                if (entityManager.HasComponent<Parent>(nextRootEntity))
                {
                    newPosition = childTransform.TransformPoint(float3.zero);

                    break;
                }
            }

            newPosition = childTransform.TransformPoint(float3.zero);
            newRotation = childTransform.TransformRotation(quaternion.identity);

            localEntity = parent;

            hasParent = entityManager.HasComponent<Parent>(localEntity);
        }

        return new LocalTransform() { Position = newPosition, Rotation = newRotation, Scale = 1 };
    }

    public static LocalTransform ConvertLocalEntityTransformToGlobalTransform(this LocalTransform entityLocalToWorld, Entity localEntity, EntityManager entityManager)
    {
        float3 newPosition = entityLocalToWorld.Position;
        Quaternion newRotation = entityLocalToWorld.Rotation;

        bool hasParent = entityManager.HasComponent<Parent>(localEntity);

        while (hasParent)
        {
            Entity parent = entityManager.GetComponentData<Parent>(localEntity).Value;

            LocalToWorld childLocalToWorld = entityManager.GetComponentData<LocalToWorld>(localEntity);

            LocalTransform childTransform = new() { Position = childLocalToWorld.Position, Rotation = childLocalToWorld.Rotation, Scale = 1 };

            if (entityManager.HasComponent<Parent>(localEntity))
            {
                Entity nextRootEntity = entityManager.GetComponentData<Parent>(localEntity).Value;

                if (entityManager.HasComponent<Parent>(nextRootEntity))
                {
                    newPosition = childTransform.TransformPoint(float3.zero);

                    break;
                }
            }

            newPosition = childTransform.TransformPoint(float3.zero);
            newRotation = childTransform.TransformRotation(quaternion.identity);

            localEntity = parent;

            hasParent = entityManager.HasComponent<Parent>(localEntity);
        }

        return new LocalTransform() { Position = newPosition, Rotation = newRotation, Scale = 1 };
    }

    public static LocalTransform GetGlobalTransform(this LocalTransform entityLocalToWorld, Entity localEntity, ComponentLookup<LocalTransform> localTransformLookup, 
         ComponentLookup<LocalToWorld> localToWorldLookup, ComponentLookup<Parent> parentLookup)
    {
        float3 newPosition = entityLocalToWorld.Position;
        Quaternion newRotation = entityLocalToWorld.Rotation;

        bool hasParent = parentLookup.HasComponent(localEntity);

        while (hasParent)
        {
            Entity parent = parentLookup.GetRefRO(localEntity).ValueRO.Value;

            RefRO<LocalToWorld> childLocalToWorld = localToWorldLookup.GetRefRO(localEntity);

            LocalTransform childTransform = new() { Position = childLocalToWorld.ValueRO.Position, Rotation = childLocalToWorld.ValueRO.Rotation, Scale = 1 };

            if (parentLookup.HasComponent(localEntity))
            {
                Entity nextRootEntity = parentLookup.GetRefRO(localEntity).ValueRO.Value;

                if (parentLookup.HasComponent(nextRootEntity))
                {
                    newPosition = childTransform.TransformPoint(float3.zero);

                    break;
                }
            }

            newPosition = childTransform.TransformPoint(float3.zero);
            newRotation = childTransform.TransformRotation(quaternion.identity);

            localEntity = parent;

            hasParent = parentLookup.HasComponent(localEntity);
        }

        return new LocalTransform() { Position = newPosition, Rotation = newRotation, Scale = 1 };
    }
}
