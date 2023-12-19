using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(FixedWingStateSystem))]
[BurstCompile]
public partial struct FixedWingEngineSystem : ISystem
{
    private EntityQuery networkEntityQuery;

    private ComponentLookup<EngineComponent> engineComponentLookup;

    private ComponentLookup<LocalTransform> localTransformLookup;

    private ComponentLookup<LocalToWorld> localToWorldLookup;

    private ComponentLookup<Parent> parentLookup;

    public void OnCreate(ref SystemState systemState)
    {
        networkEntityQuery = systemState.GetEntityQuery(ComponentType.ReadWrite<FixedWingComponent>(), ComponentType.ReadWrite<PhysicsMass>(), ComponentType.ReadWrite<PhysicsVelocity>(), 
            ComponentType.ReadOnly<FixedWingInputComponent>(), ComponentType.ReadOnly<LocalTransform>(), ComponentType.ReadOnly<LocalOwnedNetworkedEntityComponent>());

        engineComponentLookup = systemState.GetComponentLookup<EngineComponent>();

        localTransformLookup = systemState.GetComponentLookup<LocalTransform>(true);

        localToWorldLookup = systemState.GetComponentLookup<LocalToWorld>(true);

        parentLookup = systemState.GetComponentLookup<Parent>(true);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState systemState)
    {
        if (!SystemAPI.TryGetSingleton(out NetworkManagerEntityComponent networkManagerEntityComponent)) return;

        engineComponentLookup.Update(ref systemState);
        localTransformLookup.Update(ref systemState);
        localToWorldLookup.Update(ref systemState);
        parentLookup.Update(ref systemState);

        EntityCommandBuffer entityCommandBuffer = new(Allocator.TempJob);

        EntityCommandBuffer.ParallelWriter parallelWriterEntityCommandBuffer = entityCommandBuffer.AsParallelWriter();

        FixedWingEngineJob fixedWingEnginePowerJob = new() 
        { 
            deltaTime = SystemAPI.Time.DeltaTime, 
            engineComponentLookup = engineComponentLookup, 
            localTransformLookup = localTransformLookup,
            localToWorldLookup = localToWorldLookup,
            parentLookup = parentLookup,
            parallelWriterEntityCommandBuffer = parallelWriterEntityCommandBuffer 
        };

        if (networkManagerEntityComponent.NetworkType == NetworkType.None)
        {
            fixedWingEnginePowerJob.ScheduleParallel(systemState.Dependency).Complete();
        }
        else
        {
            fixedWingEnginePowerJob.ScheduleParallel(networkEntityQuery, systemState.Dependency).Complete();
        }

        entityCommandBuffer.Playback(systemState.EntityManager);
        entityCommandBuffer.Dispose();
    }

    [StructLayout(LayoutKind.Auto)]
    [BurstCompile]
    partial struct FixedWingEngineJob : IJobEntity
    {
        [ReadOnly] public float deltaTime;

        [ReadOnly] public ComponentLookup<EngineComponent> engineComponentLookup;

        [NativeDisableContainerSafetyRestriction]
        [ReadOnly] 
        public ComponentLookup<LocalTransform> localTransformLookup;

        [ReadOnly] public ComponentLookup<LocalToWorld> localToWorldLookup;

        [ReadOnly] public ComponentLookup<Parent> parentLookup;

        public EntityCommandBuffer.ParallelWriter parallelWriterEntityCommandBuffer;

        public void Execute([ChunkIndexInQuery] int sortKey, ref FixedWingComponent fixedWingComponent, ref PhysicsMass physicsMass, ref PhysicsVelocity physicsVelocity, in FixedWingInputComponent fixedWingInputComponent, in LocalTransform localTransform)
        {
            foreach (Entity engineEntity in fixedWingComponent.engineEntities)
            {
                EngineComponent engineComponent = engineComponentLookup[engineEntity];

                LocalTransform engineLocalTransform = localTransformLookup[engineEntity];

                engineComponent.currentPower = engineComponent.maxAfterBurnerPowerNewtons * fixedWingInputComponent.throttle;

                parallelWriterEntityCommandBuffer.SetComponent(sortKey, engineEntity, engineComponent);

                physicsVelocity.ApplyImpulse(physicsMass, physicsMass.Transform.pos, physicsMass.Transform.rot, 
                    engineLocalTransform.GetGlobalTransform(engineEntity, localTransformLookup, localToWorldLookup, parentLookup).Forward() * engineComponent.currentPower / 10 * deltaTime, 
                    engineLocalTransform.Position);

                /*physicsVelocity.ApplyImpulse(physicsMass, physicsMass.Transform.pos, physicsMass.Transform.rot, engineLocalTransform.TransformTransform(localTransform).Forward() * 
                    engineComponent.currentPower * deltaTime, engineLocalTransform.Position);*/
            }
        }
    }
}