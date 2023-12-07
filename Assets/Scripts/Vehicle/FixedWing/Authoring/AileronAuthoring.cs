using Unity.Entities;
using UnityEngine;

public class AileronAuthoring : MonoBehaviour
{
    public int positionId;

    public float maxPositivePitchAuthorityDegrees;

    public float maxNegativePitchAuthorityDegrees;

    public float area;

    class Baking : Baker<AileronAuthoring>
    {
        public override void Bake(AileronAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new StabilatorComponent() { Id = authoring.positionId, maxPositivePitchAuthorityDegrees = authoring.maxPositivePitchAuthorityDegrees, 
                maxNegativePitchAuthorityDegrees = authoring.maxNegativePitchAuthorityDegrees, area = authoring.area });
        }
    }
}