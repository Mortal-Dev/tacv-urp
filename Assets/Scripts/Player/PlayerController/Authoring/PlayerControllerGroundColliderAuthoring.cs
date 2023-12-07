using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class PlayerControllerGroundColliderAuthoring : MonoBehaviour
{
    class Baker : Baker<PlayerControllerGroundColliderAuthoring>
    {
        public override void Bake(PlayerControllerGroundColliderAuthoring playerControllerGroundColliderAuthoring)
        {
            TransformUsageFlags transformUsageFlags = new TransformUsageFlags();

            Entity entity = GetEntity(transformUsageFlags);

            AddComponent(entity, new PlayerControllerGroundCollisionComponent());
        }
    }
}
