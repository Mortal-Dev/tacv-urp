using Unity.Entities;
using Unity.Physics;

public partial struct StoredChildedNetworkEntityCollider : IComponentData
{
    public PhysicsCollider physicsCollider;
}