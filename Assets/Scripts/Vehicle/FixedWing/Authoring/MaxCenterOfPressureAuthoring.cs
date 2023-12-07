using Unity.Entities;
using UnityEngine;

public class MaxCenterOfPressureAuthoring : MonoBehaviour
{
    public int positionId;

    class Baking : Baker<MaxCenterOfPressureAuthoring>
    {
        public override void Bake(MaxCenterOfPressureAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new MaxCenterOfPressureComponent() { Id = authoring.positionId });
        }
    }
}