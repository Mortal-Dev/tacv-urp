using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public static class PhysicsVelocityExtensions
{
    public static float3 GetLocalLinearVelocity(this PhysicsVelocity physicsVelocity, Entity root, Entity child, ref ComponentLookup<Parent> parentLookup, 
        ref ComponentLookup<LocalTransform> localTransformLookup, int maxChildDepthIterations = 50)
    {
        RefRO<LocalTransform> rootTransform = localTransformLookup.GetRefRO(root);

        float3 localVelocity = rootTransform.ValueRO.InverseTransformDirection(physicsVelocity.Linear);

        FixedList512Bytes<Entity> entities = new() { child };

        Entity traversalEntity = child;

        int count = 0;

        //get line of children to root
        while (count < maxChildDepthIterations && traversalEntity != root)
        {
            RefRO<Parent> parent = parentLookup.GetRefRO(traversalEntity);

            entities.Add(parent.ValueRO.Value);

            traversalEntity = parent.ValueRO.Value;

            count++;
        }

        //remove root, which we already have
        entities.RemoveAt(entities.Length - 1);

        //get our local velocity by transforming the child to local
        for (int i = entities.Length - 1; i >= 0; i--)
        {
            RefRO<LocalTransform> localTransform = localTransformLookup.GetRefRO(entities[i]);

            localVelocity = localTransform.ValueRO.InverseTransformDirection(localVelocity);
        }

        return localVelocity;
    }

}