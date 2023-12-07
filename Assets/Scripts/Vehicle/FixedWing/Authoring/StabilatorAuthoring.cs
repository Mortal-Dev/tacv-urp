using UnityEngine;
using Unity.Entities;

public class StabilatorAuthoring : MonoBehaviour
{
    public int positionId;

    public float area;

    public float maxPositivePitchAuthorityDegrees;

    public float maxNegativePitchAuthorityDegrees;

    class Baking : Baker<StabilatorAuthoring>
    {
        public override void Bake(StabilatorAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new StabilatorComponent() {Id = authoring.positionId, area = authoring.area, maxPositivePitchAuthorityDegrees = authoring.maxPositivePitchAuthorityDegrees, 
                maxNegativePitchAuthorityDegrees = authoring.maxNegativePitchAuthorityDegrees});
        }
    }
}
