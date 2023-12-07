using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class LeftHandAuthoring : MonoBehaviour
{
    class Baking : Baker<LeftHandAuthoring>
    {
        public override void Bake(LeftHandAuthoring leftHandAuthoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new LeftHandComponent());
            AddComponent(entity, new HandComponent());
        }
    }
}
