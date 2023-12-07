using System.Collections.Generic;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using UnityEngine;

public class NetworkedEntityChildAuthoring : MonoBehaviour
{
    class Baking : Baker<NetworkedEntityChildAuthoring>
    {
        System.Random random = new System.Random();

        public override void Bake(NetworkedEntityChildAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            NetworkedEntityChildComponent component = new NetworkedEntityChildComponent() { Id = random.Next(int.MinValue, int.MaxValue) };

            if (component.Id == 0) component.Id = random.Next(int.MinValue, int.MaxValue);

            AddComponent(entity, component);
            AddComponent(entity, new PreviousLocalTransformRecordComponent() { localTransformRecord = new LocalTransform() { Position = authoring.transform.localPosition, Rotation = authoring.transform.localRotation, Scale = authoring.transform.localScale.y } });
        }
    }
}