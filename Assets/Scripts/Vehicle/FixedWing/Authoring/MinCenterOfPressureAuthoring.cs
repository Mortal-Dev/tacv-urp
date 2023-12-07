using Unity.Entities;
using UnityEngine;

public class MinCenterOfPressureAuthoring : MonoBehaviour
{
    public int positionId;

    class Baking : Baker<MinCenterOfPressureAuthoring>
    {
        public override void Bake(MinCenterOfPressureAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new MinCenterOfPressureComponent() { Id = authoring.positionId });
        }
    }
}