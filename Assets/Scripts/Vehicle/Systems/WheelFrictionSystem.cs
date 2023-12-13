using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateAfter(typeof(VehicleInitializeSystem))]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct WheelFrictionSystem : ISystem
{
    private ComponentLookup<VehicleComponent> vehicleComponentLookup;

    private ComponentLookup<PhysicsCollider> physicsColliderComponentLookup;

    private ComponentLookup<WheelComponent> wheelComponentLookup;

    private BufferLookup<PhysicsColliderKeyEntityPair> physicsColliderKeyEntityPairBufferLookup;

    public void OnCreate(ref SystemState systemState)
    {
        vehicleComponentLookup = systemState.GetComponentLookup<VehicleComponent>(true);
        physicsColliderComponentLookup = systemState.GetComponentLookup<PhysicsCollider>(true);
        wheelComponentLookup = systemState.GetComponentLookup<WheelComponent>(true);
        physicsColliderKeyEntityPairBufferLookup = systemState.GetBufferLookup<PhysicsColliderKeyEntityPair>(true);
    }

    public void OnUpdate(ref SystemState systemState)
    {
        if (SystemAPI.TryGetSingleton(out SimulationSingleton simulationSingleton))
        {
            vehicleComponentLookup.Update(ref systemState);
            physicsColliderComponentLookup.Update(ref systemState);
            wheelComponentLookup.Update(ref systemState);
            physicsColliderKeyEntityPairBufferLookup.Update(ref systemState);

            new WheelCollisionJob()
            {
                vehicleComponentLookup = vehicleComponentLookup,
                physicsColliderLookup = physicsColliderComponentLookup,
                wheelComponentLookup = wheelComponentLookup,
                physicsColliderKeyEntityPairs = physicsColliderKeyEntityPairBufferLookup

            }.Schedule(simulationSingleton, simulationSingleton.AsSimulation().FinalJobHandle).Complete();
        }

        return;

        foreach (var (vehicleComponent, vehicleEntity) in SystemAPI.Query<RefRO<VehicleComponent>>().WithEntityAccess())
        {
            var colliderBuffer = SystemAPI.GetBuffer<PhysicsColliderKeyEntityPair>(vehicleEntity);



            UpdateVehicleWheelsFriction(vehicleEntity, vehicleComponent, ref systemState);
        }
    }

    private void UpdateVehicleWheelsFriction(Entity vehicleEntity, RefRO<VehicleComponent> vehicleComponent, ref SystemState systemState)
    {
        //try getting collision keys from vehicle entity in initialization

        RefRW<PhysicsVelocity> vehiclePhysicsVelocity = SystemAPI.GetComponentRW<PhysicsVelocity>(vehicleEntity);
        LocalTransform vehicleLocalTransform = SystemAPI.GetComponent<LocalTransform>(vehicleEntity);
        PhysicsMass vehiclePhysicsMass = SystemAPI.GetComponent<PhysicsMass>(vehicleEntity);

        foreach (Entity wheelEntity in vehicleComponent.ValueRO.wheels)
        {
            LocalTransform wheelLocalTransform = SystemAPI.GetComponent<LocalTransform>(wheelEntity);
            RefRW<WheelComponent> wheelComponent = SystemAPI.GetComponentRW<WheelComponent>(wheelEntity);

            wheelComponent.ValueRW.rpm -= ((wheelComponent.ValueRO.rpm * math.PI) + -vehicleLocalTransform.InverseTransformDirection(vehiclePhysicsVelocity.ValueRO.Linear).z) * SystemAPI.Time.DeltaTime * 0.1f;

            Vector3 tractionForce = Vector3.ClampMagnitude(wheelComponent.ValueRO.traction * (wheelLocalTransform.Right() *
                -wheelLocalTransform.InverseTransformDirection(vehiclePhysicsVelocity.ValueRO.Linear).x + wheelLocalTransform.Forward() * ((wheelComponent.ValueRO.rpm * Mathf.PI) +
                -wheelLocalTransform.InverseTransformDirection(vehiclePhysicsVelocity.ValueRO.Linear).z)), wheelComponent.ValueRO.maxTraction);

            vehiclePhysicsVelocity.ValueRW.ApplyImpulse(in vehiclePhysicsMass, vehiclePhysicsMass.Transform.pos, vehiclePhysicsMass.Transform.rot, tractionForce * SystemAPI.Time.DeltaTime, wheelLocalTransform.Position);
        }
    }

    struct WheelCollisionJob : ICollisionEventsJob
    {
        [ReadOnly] public ComponentLookup<VehicleComponent> vehicleComponentLookup;

        [ReadOnly] public ComponentLookup<PhysicsCollider> physicsColliderLookup;

        [ReadOnly] public ComponentLookup<WheelComponent> wheelComponentLookup;

        [ReadOnly] public BufferLookup<PhysicsColliderKeyEntityPair> physicsColliderKeyEntityPairs;

        public void Execute(CollisionEvent collisionEvent)
        {

            Entity vehicleEntity = GetVehicleEntity(collisionEvent, out ColliderKey vehicleColliderKey);

            if (vehicleEntity == Entity.Null) return;

            if (!physicsColliderLookup.HasComponent(vehicleEntity)) return;

            Debug.Log(vehicleColliderKey);

            Entity wheelEntity = GetWheelChildCollidedEntity(vehicleEntity, ref vehicleColliderKey);

            if (wheelEntity == Entity.Null) return;
        }

        private Entity GetWheelChildCollidedEntity(Entity vehicleEntity, ref ColliderKey vehicleColliderKey)
        {
            if (!physicsColliderKeyEntityPairs.TryGetBuffer(vehicleEntity, out DynamicBuffer<PhysicsColliderKeyEntityPair> physicsColliderKeyEntityPairBuffer)) return Entity.Null;

            foreach (PhysicsColliderKeyEntityPair physicsColliderKeyEntityPair in physicsColliderKeyEntityPairBuffer)
            {
                if (physicsColliderKeyEntityPair.Key == vehicleColliderKey) return physicsColliderKeyEntityPair.Entity;
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
    }
}