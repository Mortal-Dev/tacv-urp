using Unity.Entities;
using Unity.Physics;
using Unity.Collections;
using Unity.Mathematics;

public static class PhysicsHelper
{
    public static NativeList<ColliderCastHit> SphereCast(CollisionFilter collisionFilter, float3 rayFrom, float3 direction, float radius, float length, CollisionWorld collisionWorld)
    {
        NativeList<ColliderCastHit> colliderHits = new NativeList<ColliderCastHit>(Allocator.TempJob);

        collisionWorld.SphereCastAll(rayFrom, radius, direction, length, ref colliderHits, collisionFilter);

        return colliderHits;
    }
}
