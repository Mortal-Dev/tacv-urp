using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

public partial struct FixedWingComponent : IComponentData
{
    public float throttle;

    public float angleOfAttack;

    public float angleOfAttackYaw;

    public float3 gForce;

    public float3 maximumGForces;

    public float3 lastVelocity;

    public float3 localVelocity;

    public float3 localAngularVelocity;

    public Entity centerOfPressureEntity;

    public Entity centerOfGravityEntity;

    public FixedList128Bytes<Entity> engineEntities;

    public FixedList128Bytes<Entity> rudderEntities;

    public FixedList128Bytes<Entity> flapEntities;

    public FixedList128Bytes<Entity> stabilatorEntities;

    public FixedList128Bytes<Entity> airleronEntities;

    public FixedList128Bytes<Entity> liftGeneratingSurfaceEntities;
}