using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class RightHandAuthoring : MonoBehaviour
{
    class Baking : Baker<RightHandAuthoring>
    {
        public override void Bake(RightHandAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new RightHandComponent());
            AddComponent(entity, new HandComponent());
        }
    }
}
