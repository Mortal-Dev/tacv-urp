using Unity.Entities;

public partial struct NetworkManagerEntityComponent : IComponentData
{
    public NetworkType NetworkType;

    public ushort localNetworkId;
}