using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct WheelFrictionSystem : ISystem
{
    public void OnUpdate(ref SystemState systemState) 
    {
        foreach (var (vehicleComponent, vehicleEntity) in SystemAPI.Query<RefRO<VehicleComponent>>().WithEntityAccess())
        {
            foreach (Entity wheelEntity in vehicleComponent.ValueRO.wheels)
                ApplyWheelFriction(wheelEntity, vehicleEntity, ref systemState);
        }
    }

    private void ApplyWheelFriction(Entity wheelEntity, Entity vehicleEntity, ref SystemState systemState)
    {
        LocalTransform wheelLocalTransform = SystemAPI.GetComponent<LocalTransform>(wheelEntity);
        WheelComponent wheelComponent = SystemAPI.GetComponent<WheelComponent>(wheelEntity);
        PhysicsMass physicsMass = SystemAPI.GetComponent<PhysicsMass>(vehicleEntity);
        RefRW<PhysicsVelocity> vehiclePhysicsVelocity = SystemAPI.GetComponentRW<PhysicsVelocity>(vehicleEntity);

        Vector3 tractionForce = Vector3.ClampMagnitude(wheelComponent.traction * (wheelLocalTransform.Right() *
            -wheelLocalTransform.InverseTransformDirection(vehiclePhysicsVelocity.ValueRO.Linear).x + wheelLocalTransform.Forward() * ((wheelComponent.rpm * Mathf.PI) +
            -wheelLocalTransform.InverseTransformDirection(vehiclePhysicsVelocity.ValueRO.Linear).z)), wheelComponent.maxTraction);
        
        vehiclePhysicsVelocity.ValueRW.ApplyImpulse(in physicsMass, physicsMass.Transform.pos, physicsMass.Transform.rot, tractionForce * SystemAPI.Time.DeltaTime, wheelLocalTransform.Position);
    }
}