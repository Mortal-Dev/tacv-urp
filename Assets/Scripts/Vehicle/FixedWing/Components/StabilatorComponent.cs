using Unity.Entities;

public partial struct StabilatorComponent : IComponentData, ComponentId
{
    public int Id { get; set; }

    public float currentPitchDegrees;

    public float maxPositivePitchAuthorityDegrees;

    public float maxNegativePitchAuthorityDegrees;

    public float area;
}
