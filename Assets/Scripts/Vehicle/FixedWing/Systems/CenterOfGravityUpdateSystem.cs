using UnityEngine;
using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(FixedWingInitializeSystem))]
[BurstCompile]
public partial struct CenterOfGravityUpdateSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState systemState)
    {
        EntityCommandBuffer entityCommandBuffer = new(Allocator.Temp);

        foreach (var (parent, localTransform) in SystemAPI.Query<RefRO<Parent>, RefRO<LocalTransform>>().WithAll<CenterOfGravityComponent>())
        {
            PhysicsMass physicsMass = SystemAPI.GetComponent<PhysicsMass>(parent.ValueRO.Value);

            physicsMass.CenterOfMass = localTransform.ValueRO.Position;

            PhysicsVelocity physicsVelocity = SystemAPI.GetComponent<PhysicsVelocity>(parent.ValueRO.Value);

           // physicsVelocity.Angular.y = 0;
           // physicsVelocity.Angular.z = 0;

            entityCommandBuffer.SetComponent(parent.ValueRO.Value, physicsMass);
            entityCommandBuffer.SetComponent(parent.ValueRO.Value, physicsVelocity);
        }

        entityCommandBuffer.Playback(systemState.EntityManager);
        entityCommandBuffer.Dispose();
    }
}