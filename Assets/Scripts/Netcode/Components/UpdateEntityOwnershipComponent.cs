using Unity.Entities;

public partial struct UpdateEntityOwnershipComponent : IComponentData
{
    public ushort newOwnerConnectionId;
}