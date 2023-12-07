using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Unity.Physics.Extensions;
using Unity.Collections;
using System;
using UnityEngine;
using Unity.Jobs;
using Unity.Physics.Systems;
using static UnityEngine.EventSystems.EventTrigger;

[ClientSystem]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[BurstCompile]
public partial struct PlayerMovementSystem : ISystem
{
    EntityQuery networkEntityQuery;

    public void OnCreate(ref SystemState systemState)
    {
        networkEntityQuery = systemState.GetEntityQuery(ComponentType.ReadWrite<PlayerControllerComponent>(), ComponentType.ReadOnly<PlayerControllerInputComponent>(), ComponentType.ReadOnly<Simulate>(),
            ComponentType.ReadOnly<LocalOwnedNetworkedEntityComponent>());
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState systemState)
    {
        if (!SystemAPI.TryGetSingleton(out NetworkManagerEntityComponent networkManagerEntityComponent)) return;

        if (networkManagerEntityComponent.NetworkType == NetworkType.None)
        {

        }

       /* SetPlayerRotation(ref systemState);

        foreach (var (playerController, entity) in SystemAPI.Query<RefRW<PlayerControllerComponent>>().WithAll<PlayerControllerInputComponent>().WithAll<Simulate>().WithEntityAccess()
            .WithAll<LocalOwnedNetworkedEntityComponent>())
        {
            switch (playerController.ValueRO.playerState)
            {
                case PlayerState.Moving:
                    PlayerControllerMoving(playerController,
                        SystemAPI.GetComponentRW<PhysicsVelocity>(entity),
                        SystemAPI.GetComponentRW<LocalTransform>(entity),
                        SystemAPI.GetComponentRO<PlayerControllerInputComponent>(entity));
                    break;

                case PlayerState.InAir:
                    PlayerControllerInAir(playerController,
                        SystemAPI.GetComponentRW<PhysicsVelocity>(entity));
                    break;
            }

            return;
        }

        foreach (var (playerController, entity) in SystemAPI.Query<RefRW<PlayerControllerComponent>>().WithAll<PlayerControllerInputComponent>().WithAll<Simulate>().WithEntityAccess())
        {
            switch (playerController.ValueRO.playerState)
            {
                case PlayerState.Moving:
                    PlayerControllerMoving(playerController,
                        SystemAPI.GetComponentRW<PhysicsVelocity>(entity),
                        SystemAPI.GetComponentRW<LocalTransform>(entity),
                        SystemAPI.GetComponentRO<PlayerControllerInputComponent>(entity));
                    break;

                case PlayerState.InAir:
                    PlayerControllerInAir(playerController,
                        SystemAPI.GetComponentRW<PhysicsVelocity>(entity));
                    break;
            }
        }*/
    }

    [BurstCompile]
    partial struct PlayerMovementJob : IJobEntity
    {
        public void Execute(ref PlayerControllerComponent playerControllerComponent, PlayerControllerInputComponent playerControllerInputComponent, ref PhysicsVelocity physicsVelocity, 
            ref LocalTransform localTransform)
        {
            switch (playerControllerComponent.playerState)
            {
                case PlayerState.Moving:
                    PlayerControllerMoving(ref playerControllerComponent, ref physicsVelocity, ref localTransform, ref playerControllerInputComponent);
                    break;

                case PlayerState.InAir:
                    PlayerControllerInAir(ref playerControllerComponent, ref physicsVelocity);
                    break;
            }
        }

        [BurstCompile]
        private void PlayerControllerMoving(ref PlayerControllerComponent playerController, ref PhysicsVelocity physicsVelocity, ref LocalTransform localTransform, ref PlayerControllerInputComponent input)
        {
            //forwards/backwards
            physicsVelocity.Linear += localTransform.Forward() * input.leftControllerThumbstick.y * (input.leftControllerThumbstick.y > 0 ? playerController.forwardForce : playerController.backwardForce);

            //left/right
            physicsVelocity.Linear += localTransform.Right() * input.leftControllerThumbstick.x * playerController.sideForce;

            //cap velocities
            Vector3 velocityVector3 = physicsVelocity.Linear;

            if (input.leftControllerThumbstick.y > 0 && velocityVector3.magnitude > playerController.maxForwardVelocity)
                physicsVelocity.Linear = velocityVector3.normalized * playerController.maxForwardVelocity;
            else if (input.leftControllerThumbstick.y < 0 && velocityVector3.magnitude > playerController.maxBackwardsVelocity)
                physicsVelocity.Linear = velocityVector3.normalized * playerController.maxBackwardsVelocity;
            else if (input.leftControllerThumbstick.x != 0 && velocityVector3.magnitude > playerController.maxSideVelocity)
                physicsVelocity.Linear = velocityVector3.normalized * playerController.maxSideVelocity;

            //physicsVelocity.ValueRW.Linear = velocityVector3;

            //remove angular velocity
            physicsVelocity.Angular = float3.zero;
        }

        [BurstCompile]
        private void PlayerControllerInAir(ref PlayerControllerComponent playerController, ref PhysicsVelocity physicsVelocity)
        {
            if (physicsVelocity.Linear.y > playerController.maxDownVelocity)
                physicsVelocity.Linear = playerController.maxDownVelocity;
        }

        [BurstCompile]
        private void SetPlayerRotation(ref SystemState systemState)
        {
          /*  foreach (var (characterControllerLocalTransform, entity) in SystemAPI.Query<RefRW<LocalTransform>>().WithAll<PlayerControllerComponent>().WithEntityAccess())
            {

                DynamicBuffer<LinkedEntityGroup> dynamicLinkedEntityGroupBuffer = systemState.EntityManager.GetBuffer<LinkedEntityGroup>(entity);

                for (int i = 0; i < dynamicLinkedEntityGroupBuffer.Length; i++)
                {
                    LinkedEntityGroup linkedEntityGroup = dynamicLinkedEntityGroupBuffer[i];

                   // if (!SystemAPI.HasComponent<HeadComponent>(linkedEntityGroup.Value)) continue;

          //          RefRO<LocalTransform> headLocalTransform = SystemAPI.GetComponentRO<LocalTransform>(entity);
//
                    characterControllerLocalTransform.ValueRW.Position = headLocalTransform.ValueRO.Position;
                    characterControllerLocalTransform.ValueRW.Rotation = headLocalTransform.ValueRO.Rotation;
                }
            }*/
        }
    }
}