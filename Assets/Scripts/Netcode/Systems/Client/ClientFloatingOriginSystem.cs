using Unity.Entities;
using Unity.Burst;

[ClientSystem]
[UpdateBefore(typeof(TickSystem))]
[BurstCompile]
public partial struct ClientFloatingOriginSystem : ISystem
{
    public void OnCreate(ref SystemState systemState)
    {
    }

    public void OnUpdate(ref SystemState systemState)
    {
        if (NetworkManager.Instance.NetworkType == NetworkType.Server) return;
    }
}