using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class HeadAuthoring : MonoBehaviour
{
    class Baking : Baker<HeadAuthoring>
    {
        public override void Bake(HeadAuthoring headAuthoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new HeadComponent());
        }
    }
}
