using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;

[UpdateInGroup(typeof(PhysicsSystemGroup))]
public partial struct WheelFrictionSystem : ISystem
{
    private ComponentLookup<VehicleComponent> vehicleComponentLookup;

    private ComponentLookup<PhysicsCollider> physicsColliderComponentLookup;

    private ComponentLookup<PhysicsMass> physicsMassComponentLookup;

    private ComponentLookup<PhysicsVelocity> physicsVelocityComponentLookup;

    private ComponentLookup<LocalTransform> localTransformComponentLookup;

    private ComponentLookup<WheelComponent> wheelComponentLookup;

    private BufferLookup<PhysicsColliderKeyEntityPair> physicsColliderKeyEntityPairBufferLookup;

    public void OnCreate(ref SystemState systemState)
    {
        vehicleComponentLookup = systemState.GetComponentLookup<VehicleComponent>(true);
        physicsColliderComponentLookup = systemState.GetComponentLookup<PhysicsCollider>(true);
        physicsMassComponentLookup = systemState.GetComponentLookup<PhysicsMass>(true);
        physicsVelocityComponentLookup = systemState.GetComponentLookup<PhysicsVelocity>();
        localTransformComponentLookup = systemState.GetComponentLookup<LocalTransform>(true);
        wheelComponentLookup = systemState.GetComponentLookup<WheelComponent>();
        physicsColliderKeyEntityPairBufferLookup = systemState.GetBufferLookup<PhysicsColliderKeyEntityPair>(true);
    }

    public void OnUpdate(ref SystemState systemState)
    {
        if (!SystemAPI.TryGetSingleton(out SimulationSingleton simulationSingleton)) return;

        vehicleComponentLookup.Update(ref systemState);
        physicsColliderComponentLookup.Update(ref systemState);
        physicsMassComponentLookup.Update(ref systemState);
        physicsVelocityComponentLookup.Update(ref systemState);
        localTransformComponentLookup.Update(ref systemState);
        wheelComponentLookup.Update(ref systemState);
        physicsColliderKeyEntityPairBufferLookup.Update(ref systemState);

        WheelCollisionJob wheelCollisionJob = new()
        {
            vehicleComponentLookup = vehicleComponentLookup,
            physicsColliderLookup = physicsColliderComponentLookup,
            physicsMassComponentLookup = physicsMassComponentLookup,
            physicsVelocityComponentLookup = physicsVelocityComponentLookup,
            localTransformComponentLookup = localTransformComponentLookup,
            wheelComponentLookup = wheelComponentLookup,
            physicsColliderKeyEntityPairs = physicsColliderKeyEntityPairBufferLookup,
            deltaTime = SystemAPI.Time.DeltaTime
        };

        wheelCollisionJob.Schedule(simulationSingleton, simulationSingleton.AsSimulation().FinalJobHandle).Complete();
    }

    struct WheelCollisionJob : ICollisionEventsJob
    {
        [ReadOnly] public ComponentLookup<VehicleComponent> vehicleComponentLookup;

        [ReadOnly] public ComponentLookup<PhysicsCollider> physicsColliderLookup;

        [ReadOnly] public ComponentLookup<PhysicsMass> physicsMassComponentLookup;

        public ComponentLookup<PhysicsVelocity> physicsVelocityComponentLookup;

        [ReadOnly] public ComponentLookup<LocalTransform> localTransformComponentLookup;

        public ComponentLookup<WheelComponent> wheelComponentLookup;

        [ReadOnly] public BufferLookup<PhysicsColliderKeyEntityPair> physicsColliderKeyEntityPairs;

        [ReadOnly] public float deltaTime;

        public void Execute(CollisionEvent collisionEvent)
        {
            Entity vehicleEntity = GetVehicleEntity(collisionEvent, out ColliderKey vehicleColliderKey);

            if (vehicleEntity == Entity.Null) return;

            if (!physicsColliderLookup.HasComponent(vehicleEntity)) return;

            Entity wheelEntity = GetWheelChildCollidedEntity(vehicleEntity, ref vehicleColliderKey);

            if (wheelEntity == Entity.Null) return;

            UpdateVehicleWheelFriction(vehicleEntity, wheelEntity);
        }

        private Entity GetWheelChildCollidedEntity(Entity vehicleEntity, ref ColliderKey vehicleColliderKey)
        {
            if (!physicsColliderKeyEntityPairs.TryGetBuffer(vehicleEntity, out DynamicBuffer<PhysicsColliderKeyEntityPair> physicsColliderKeyEntityPairBuffer)) return Entity.Null;

            foreach (PhysicsColliderKeyEntityPair physicsColliderKeyEntityPair in physicsColliderKeyEntityPairBuffer)
            {
                if (physicsColliderKeyEntityPair.Key == vehicleColliderKey && wheelComponentLookup.HasComponent(physicsColliderKeyEntityPair.Entity)) return physicsColliderKeyEntityPair.Entity;
            }

            return Entity.Null;
        }

        private Entity GetVehicleEntity(CollisionEvent collisionEvent, out ColliderKey vehicleColliderKey)
        {
            if (vehicleComponentLookup.HasComponent(collisionEvent.EntityA))
            {
                vehicleColliderKey = collisionEvent.ColliderKeyA;
                return collisionEvent.EntityA;
            }
            else if (vehicleComponentLookup.HasComponent(collisionEvent.EntityB))
            {
                vehicleColliderKey = collisionEvent.ColliderKeyB;
                return collisionEvent.EntityB;
            }

            vehicleColliderKey = ColliderKey.Empty;
            return Entity.Null;
        }

        private void UpdateVehicleWheelFriction(Entity vehicleEntity, Entity wheelEntity)
        {
            RefRW<PhysicsVelocity> vehiclePhysicsVelocity = physicsVelocityComponentLookup.GetRefRW(vehicleEntity);
            RefRO<LocalTransform> vehicleLocalTransform = localTransformComponentLookup.GetRefRO(vehicleEntity);
            RefRO<PhysicsMass> vehiclePhysicsMass = physicsMassComponentLookup.GetRefRO(vehicleEntity);

            RefRO<LocalTransform> wheelLocalTransform = localTransformComponentLookup.GetRefRO(wheelEntity);
            RefRW<WheelComponent> wheelComponent = wheelComponentLookup.GetRefRW(wheelEntity);

            wheelComponent.ValueRW.rpm -= ((wheelComponent.ValueRO.rpm * math.PI) + -vehicleLocalTransform.ValueRO.InverseTransformDirection(vehiclePhysicsVelocity.ValueRO.Linear).z) *
                deltaTime * 0.1f;

            Vector3 tractionForce = Vector3.ClampMagnitude(wheelComponent.ValueRO.traction * (wheelLocalTransform.ValueRO.Right() *
                -wheelLocalTransform.ValueRO.InverseTransformDirection(vehiclePhysicsVelocity.ValueRO.Linear).x + wheelLocalTransform.ValueRO.Forward() * ((wheelComponent.ValueRO.rpm *
                Mathf.PI) +
                -wheelLocalTransform.ValueRO.InverseTransformDirection(vehiclePhysicsVelocity.ValueRO.Linear).z)), wheelComponent.ValueRO.maxTraction);

            vehiclePhysicsVelocity.ValueRW.ApplyImpulse(in vehiclePhysicsMass.ValueRO, vehiclePhysicsMass.ValueRO.Transform.pos, vehiclePhysicsMass.ValueRO.Transform.rot, tractionForce *
                deltaTime, wheelLocalTransform.ValueRO.Position);
        }
    }
}