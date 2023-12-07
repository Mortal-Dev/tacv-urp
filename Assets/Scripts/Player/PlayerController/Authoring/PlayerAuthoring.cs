using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR;

[DisallowMultipleComponent]
public class PlayerAuthoring : MonoBehaviour
{
    class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            TransformUsageFlags transformUsageFlags = new TransformUsageFlags();

            Entity entity = GetEntity(transformUsageFlags);

            AddComponent(entity, new PlayerComponent());

            AddComponent(entity, new PlayerControllerComponent()
            {
                playerState = PlayerState.Moving,

                forwardForce = 0.6f,
                backwardForce = 0.6f,
                sideForce = 0.6f,

                maxForwardVelocity = 2f,
                maxBackwardsVelocity = 1.5f,
                maxSideVelocity = 1.5f,
            });
        }
    }
}
