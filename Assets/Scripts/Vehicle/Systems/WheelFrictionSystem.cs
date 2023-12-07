using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct WheelFrictionSystem : ISystem
{
    public readonly void OnUpdate(ref SystemState systemState) 
    {
        return;
        //iterate through vehicle instead
        foreach (var (localTransform, wheelComponent, parent) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<WheelComponent>, RefRO<Parent>>())
        {
            PhysicsVelocity physicsVelocity = SystemAPI.GetComponent<PhysicsVelocity>(parent.ValueRO.Value);

            Vector3 tractionForce = Vector3.ClampMagnitude(wheelComponent.ValueRO.traction * (localTransform.ValueRW.Right() * 
                -localTransform.ValueRW.InverseTransformDirection(physicsVelocity.Linear).x + localTransform.ValueRW.Forward() * ((wheelComponent.ValueRO.rpm * Mathf.PI) + 
                -localTransform.ValueRW.InverseTransformDirection(physicsVelocity.Linear).z)), wheelComponent.ValueRO.maxTraction);


        }
    }
}