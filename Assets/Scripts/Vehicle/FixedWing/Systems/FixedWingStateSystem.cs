using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Extensions;
using UnityEngine;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct FixedWingStateSystem : ISystem
{
    private float deltaTime;

    private Quaternion inverseRotation;

    public void OnUpdate(ref SystemState systemState)
    {
        deltaTime = ((FixedStepSimulationSystemGroup)systemState.World.GetExistingSystemManaged(typeof(FixedStepSimulationSystemGroup))).Timestep;

        if (NetworkManager.Instance.NetworkType == NetworkType.None)
        {
            foreach (var (fixedWingComponent, localTransform, velocity) in SystemAPI.Query<RefRW<FixedWingComponent>, RefRO<LocalTransform>,
            RefRW<PhysicsVelocity>>().WithNone<UninitializedFixedWingComponent>())
            {
                UpdateFixedWing(fixedWingComponent, localTransform, velocity, ref systemState);
            }
        }
        else
        {
            foreach (var (fixedWingComponent, localTransform, velocity) in SystemAPI.Query<RefRW<FixedWingComponent>, RefRO<LocalTransform>,
            RefRW<PhysicsVelocity>>().WithNone<UninitializedFixedWingComponent>().WithAll<LocalOwnedNetworkedEntityComponent>())
            {
                UpdateFixedWing(fixedWingComponent, localTransform, velocity, ref systemState);
            }
        }
    }

    private void UpdateFixedWing(RefRW<FixedWingComponent> fixedWingComponent, RefRO<LocalTransform> localTransformComponent, 
        RefRW<PhysicsVelocity> physicsVelocityComponent, ref SystemState systemState)
    {
        inverseRotation = Quaternion.Inverse(localTransformComponent.ValueRO.Rotation);

        SetGForce(fixedWingComponent, physicsVelocityComponent);

        CalculatePhysics(localTransformComponent, physicsVelocityComponent, fixedWingComponent);
    }

    private void CalculatePhysics(RefRO<LocalTransform> localTransform, RefRW<PhysicsVelocity> physicsVelocity, RefRW<FixedWingComponent> fixedWingComponent)
    {
        var invRotation = Quaternion.Inverse(localTransform.ValueRO.Rotation);

       // fixedWingComponent.ValueRW.localVelocity = localTransform.ValueRO.InverseTransformDirection(physicsVelocity.ValueRO.Linear);

        fixedWingComponent.ValueRW.localVelocity = invRotation * physicsVelocity.ValueRO.Linear;  //transform world velocity into local space
        fixedWingComponent.ValueRW.localAngularVelocity = invRotation * physicsVelocity.ValueRO.Angular;  //transform into local space
            
        SetAngleOfAttack(fixedWingComponent);
    }

    private void SetAngleOfAttack(RefRW<FixedWingComponent> fixedWingComponent)
    {
        if (((Vector3)fixedWingComponent.ValueRO.localVelocity).sqrMagnitude < 0.0001f)
        {
            fixedWingComponent.ValueRW.angleOfAttack = 0;
            fixedWingComponent.ValueRW.angleOfAttackYaw = 0;
            return;
        }

        fixedWingComponent.ValueRW.angleOfAttack =  math.degrees(math.atan2(-fixedWingComponent.ValueRO.localVelocity.y, fixedWingComponent.ValueRO.localVelocity.z));
        fixedWingComponent.ValueRW.angleOfAttackYaw = math.degrees(math.atan2(fixedWingComponent.ValueRO.localVelocity.x, fixedWingComponent.ValueRO.localVelocity.z));

        //Debug.Log("angel of attack: " + fixedWingComponent.ValueRW.angleOfAttack);
    }

    private void SetGForce(RefRW<FixedWingComponent> fixedWingComponent, RefRW<PhysicsVelocity> physicsVelocity)
    {
        float3 acceleration = (physicsVelocity.ValueRO.Linear - fixedWingComponent.ValueRO.lastVelocity) / deltaTime;

        fixedWingComponent.ValueRW.lastVelocity = physicsVelocity.ValueRO.Linear;

        fixedWingComponent.ValueRW.gForce = inverseRotation * acceleration;
    }
}
