using Unity.Entities;

public partial struct EngineComponent : IComponentData, ComponentId
{
    public int Id { get; set; }

    public float maxMilitaryPowerNewtons;

    public float maxAfterBurnerPowerNewtons;

    public float currentPower;
}