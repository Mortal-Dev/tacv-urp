using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class RudderAuthoring : MonoBehaviour
{
    public int positionId;

    public float area;

    public float maxRudderAngleDegrees;

    public float maxRudderDrag;

    class Baking : Baker<RudderAuthoring>
    {
        public override void Bake(RudderAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new RudderComponent() { Id = authoring.positionId, area = authoring.area, maxRudderAngleDegrees = authoring.maxRudderAngleDegrees });
        }
    }
}