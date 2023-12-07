using Unity.Entities;

[UpdateAfter(typeof(FixedStepSimulationSystemGroup))]
public partial struct TickSystem : ISystem
{
    public void OnUpdate(ref SystemState systemState)
    {
        NetworkManager.Instance.Tick();
    }
}