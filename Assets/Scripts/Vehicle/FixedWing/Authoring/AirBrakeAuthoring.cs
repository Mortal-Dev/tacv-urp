using Unity.Entities;
using UnityEngine;

public class AirBrakeAuthoring : MonoBehaviour
{
    public int positionId;

    public float maxDrag;

    public float timeToDeploy;

    class Baking : Baker<AirBrakeAuthoring>
    {
        public override void Bake(AirBrakeAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new AirBrakeComponent() { Id = authoring.positionId, maxDrag = authoring.maxDrag, timeToDeploy = authoring.timeToDeploy });
        }
    }
}