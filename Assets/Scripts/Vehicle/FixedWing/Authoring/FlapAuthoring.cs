using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class FlapAuthoring : MonoBehaviour
{
    public int positionId;

    public float maxFlapDegree;

    public float maxDrag;

    public float maxLift;

    class Baking : Baker<FlapAuthoring>
    {
        public override void Bake(FlapAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new FlapComponent() { Id = authoring.positionId, maxFlapDegree = authoring.maxFlapDegree, maxDrag = authoring.maxDrag, maxLift = authoring.maxLift });
        }
    }
}
