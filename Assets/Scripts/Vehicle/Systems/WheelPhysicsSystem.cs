using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(PhysicsSystemGroup))]
public partial struct WheelPhysicsSystem : ISystem
{
    private ComponentLookup<LocalOwnedNetworkedEntityComponent> localOwnedNetworkedEntityComponentLookup;

    [ReadOnly] public ComponentLookup<Parent> parentComponentLookup;

    private ComponentLookup<VehicleComponent> vehicleComponentLookup;

    private ComponentLookup<PhysicsCollider> physicsColliderComponentLookup;

    private ComponentLookup<PhysicsMass> physicsMassComponentLookup;

    private ComponentLookup<PhysicsVelocity> physicsVelocityComponentLookup;

    private ComponentLookup<LocalTransform> localTransformComponentLookup;

    private ComponentLookup<WheelComponent> wheelComponentLookup;

    private BufferLookup<PhysicsColliderKeyEntityPair> physicsColliderKeyEntityPairBufferLookup;

    public void OnCreate(ref SystemState systemState)
    {
        localOwnedNetworkedEntityComponentLookup = systemState.GetComponentLookup<LocalOwnedNetworkedEntityComponent>(true);
        parentComponentLookup = systemState.GetComponentLookup<Parent>(true);
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
        if (!SystemAPI.TryGetSingleton(out SimulationSingleton simulationSingleton) || simulationSingleton.Type == SimulationType.NoPhysics) return;

        localOwnedNetworkedEntityComponentLookup.Update(ref systemState);
        parentComponentLookup.Update(ref systemState);
        vehicleComponentLookup.Update(ref systemState);
        physicsColliderComponentLookup.Update(ref systemState);
        physicsMassComponentLookup.Update(ref systemState);
        physicsVelocityComponentLookup.Update(ref systemState);
        localTransformComponentLookup.Update(ref systemState);
        wheelComponentLookup.Update(ref systemState);
        physicsColliderKeyEntityPairBufferLookup.Update(ref systemState);

        WheelPhysicsJob wheelCollisionJob = new()
        {
            localOwnedNetworkedEntityComponentLookup = localOwnedNetworkedEntityComponentLookup,
            parentComponentLookup = parentComponentLookup,
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

    struct WheelPhysicsJob : ICollisionEventsJob
    {
        [ReadOnly] public ComponentLookup<LocalOwnedNetworkedEntityComponent> localOwnedNetworkedEntityComponentLookup;

        [ReadOnly] public ComponentLookup<Parent> parentComponentLookup;

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

            if (!localOwnedNetworkedEntityComponentLookup.HasComponent(vehicleEntity)) return;

            if (!physicsColliderLookup.HasComponent(vehicleEntity)) return;

            Entity wheelEntity = GetWheelChildCollidedEntity(vehicleEntity, ref vehicleColliderKey);

            if (wheelEntity == Entity.Null) return;

            UpdateWheel(vehicleEntity, wheelEntity);
        }

        private void UpdateWheel(Entity vehicleEntity, Entity wheelEntity)
        {
            RefRW<PhysicsVelocity> vehiclePhysicsVelocity = physicsVelocityComponentLookup.GetRefRW(vehicleEntity);
            RefRO<LocalTransform> vehicleLocalTransform = localTransformComponentLookup.GetRefRO(vehicleEntity);
            RefRO<PhysicsMass> vehiclePhysicsMass = physicsMassComponentLookup.GetRefRO(vehicleEntity);

            RefRO<LocalTransform> wheelLocalTransform = localTransformComponentLookup.GetRefRO(wheelEntity);
            RefRW<WheelComponent> wheelComponent = wheelComponentLookup.GetRefRW(wheelEntity);

            float3 wheelVelocity = vehiclePhysicsVelocity.ValueRO.GetLocalLinearVelocity(vehicleEntity, wheelEntity, ref parentComponentLookup, ref localTransformComponentLookup);

            UpdateVehicleWheelFriction(vehiclePhysicsVelocity, vehicleLocalTransform, vehiclePhysicsMass, wheelLocalTransform, wheelComponent, wheelVelocity);

            UpdateVehicleWheelRPM(wheelVelocity, wheelComponent);
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

        private void UpdateVehicleWheelFriction(RefRW<PhysicsVelocity> vehiclePhysicsVelocity, RefRO<LocalTransform> vehicleLocalTransform, RefRO<PhysicsMass> vehiclePhysicsMass, 
            RefRO<LocalTransform> wheelLocalTransform, RefRW<WheelComponent> wheelComponent, float3 wheelVelocity)
        {
            float sideToFowardVelocityRatio = wheelVelocity.x / (wheelVelocity.x + wheelVelocity.z);

            if (sideToFowardVelocityRatio <= 0.04) return;

            wheelVelocity.x = vehiclePhysicsMass.ValueRO.GetMass() * -wheelVelocity.x * wheelComponent.ValueRO.traction;
            wheelVelocity.z = 0;
            wheelVelocity.y = 0;

            float3 wheelFrictionForce = vehicleLocalTransform.ValueRO.TransformDirection(wheelLocalTransform.ValueRO.TransformDirection(wheelVelocity));

            vehiclePhysicsVelocity.ValueRW.ApplyImpulse(in vehiclePhysicsMass.ValueRO, vehiclePhysicsMass.ValueRO.Transform.pos, vehiclePhysicsMass.ValueRO.Transform.rot,
                wheelFrictionForce * sideToFowardVelocityRatio, wheelLocalTransform.ValueRO.Position);
        }

        private void UpdateVehicleWheelRPM(float3 wheelVelocity, RefRW<WheelComponent> wheelComponent)
        {
            float metersPerMinuteVelocity = wheelVelocity.z * 60f;

            float circumference = wheelComponent.ValueRO.radius * 2 * math.PI;

            wheelComponent.ValueRW.rpm = metersPerMinuteVelocity / circumference;
        }
    }
}