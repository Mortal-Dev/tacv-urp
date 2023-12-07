using Unity.Entities;

public partial struct AirleronComponent : IComponentData, ComponentId
{
    public int Id { get; set; }
}
