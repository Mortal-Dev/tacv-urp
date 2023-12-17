using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.SocialPlatforms;

[UpdateInGroup(typeof(PhysicsSystemGroup))]
public partial struct WheelFrictionSystem : ISystem
{
    private ComponentLookup<LocalOwnedNetworkedEntityComponent> localOwnedNetworkedEntityComponentLookup;

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
        vehicleComponentLookup.Update(ref systemState);
        physicsColliderComponentLookup.Update(ref systemState);
        physicsMassComponentLookup.Update(ref systemState);
        physicsVelocityComponentLookup.Update(ref systemState);
        localTransformComponentLookup.Update(ref systemState);
        wheelComponentLookup.Update(ref systemState);
        physicsColliderKeyEntityPairBufferLookup.Update(ref systemState);

        WheelCollisionJob wheelCollisionJob = new()
        {
            localOwnedNetworkedEntityComponentLookup = localOwnedNetworkedEntityComponentLookup,
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
        [ReadOnly] public ComponentLookup<LocalOwnedNetworkedEntityComponent> localOwnedNetworkedEntityComponentLookup;

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

            float metersPerMinuteVelocity = vehiclePhysicsVelocity.ValueRO.Linear.z * 60f;

            float circumference = wheelComponent.ValueRO.radius * 2 * math.PI;

            wheelComponent.ValueRW.rpm = metersPerMinuteVelocity / circumference;

            float3 localVelocity = vehicleLocalTransform.ValueRO.InverseTransformDirection(vehiclePhysicsVelocity.ValueRO.Linear);

            float3 wheelVelocity = wheelLocalTransform.ValueRO.InverseTransformDirection(localVelocity);
            
            float sideToFowardVelocityRatio = wheelVelocity.x / (wheelVelocity.x + wheelVelocity.z);

            if (sideToFowardVelocityRatio <= 0.04) return;

            float3 forceToStop = vehiclePhysicsMass.ValueRO.GetMass() * -vehiclePhysicsVelocity.ValueRO.Linear;

            vehiclePhysicsVelocity.ValueRW.ApplyImpulse(in vehiclePhysicsMass.ValueRO, vehiclePhysicsMass.ValueRO.Transform.pos, vehiclePhysicsMass.ValueRO.Transform.rot, 
                forceToStop * sideToFowardVelocityRatio, wheelLocalTransform.ValueRO.Position);
        }
    }
}