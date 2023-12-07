using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[DisallowMultipleComponent]
public class PlayerInputAuthoring : MonoBehaviour
{
    class Baking : Baker<PlayerInputAuthoring>
    {
        public override void Bake(PlayerInputAuthoring authoring)
        {
            TransformUsageFlags transformUsageFlags = new TransformUsageFlags();

            Entity entity = GetEntity(transformUsageFlags);

            AddComponent(entity, new PlayerControllerInputComponent());
        }
    }
}
