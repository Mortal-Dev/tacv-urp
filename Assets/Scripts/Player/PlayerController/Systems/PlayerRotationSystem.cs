using Unity.Entities;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PlayerMovementSystem))]
internal partial class PlayerRotationSystem : SystemBase
{
    protected override void OnUpdate()
    {
        
    }
}