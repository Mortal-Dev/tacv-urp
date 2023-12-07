using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Burst;
using Unity.Physics.Systems;
using Unity.Physics;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using System;
using System.Diagnostics;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[BurstCompile]
public partial struct PlayerControllerGroundCollisionSystem : ISystem
{
    EntityQuery networkedEntityQuery;

    EntityQuery physicsWorldSingletonQuery;

    public void OnCreate(ref SystemState systemState)
    {
        networkedEntityQuery = systemState.GetEntityQuery(ComponentType.ReadOnly<LocalTransform>(), ComponentType.ReadWrite<PlayerControllerComponent>(), ComponentType.ReadOnly<LocalOwnedNetworkedEntityComponent>());
        physicsWorldSingletonQuery = systemState.GetEntityQuery(ComponentType.ReadWrite<PhysicsWorldSingleton>());
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState systemState)
    {
        if (!SystemAPI.TryGetSingleton(out NetworkManagerEntityComponent networkManagerEntity)) return;

        if (!SystemAPI.HasSingleton<PhysicsWorldSingleton>()) return;

        CollisionWorld collisionWorld = physicsWorldSingletonQuery.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;

        if (networkManagerEntity.NetworkType == NetworkType.None)
        {
            new PlayerControllerGroundCollisionJob() { collisionWorld = collisionWorld }.ScheduleParallel(systemState.Dependency).Complete();
        }
        else
        {
            new PlayerControllerGroundCollisionJob() { collisionWorld = collisionWorld }.ScheduleParallel(networkedEntityQuery, systemState.Dependency).Complete();
        }
    }

    [BurstCompile]
    partial struct PlayerControllerGroundCollisionJob : IJobEntity
    {
        [ReadOnly] public CollisionWorld collisionWorld;

        public void Execute(in LocalTransform localTransform, ref PlayerControllerComponent playerControllerComponent)
        {
            NativeList<ColliderCastHit> sphereCastColliderHits = PhysicsHelper.SphereCast(CollisionFilter.Default, localTransform.Position, -localTransform.Up(), 0.2f, 1.1f, collisionWorld);

            //we always collide with ourselves, so if there's more, we are on the ground
            if (sphereCastColliderHits.Length > 1)
                playerControllerComponent.playerState = PlayerState.Moving;
            else
                playerControllerComponent.playerState = PlayerState.InAir;

            sphereCastColliderHits.Dispose();
        }
    }
}