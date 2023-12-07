using Unity.Entities;


public partial struct NetworkedParentRequestComponent : IComponentData
{
    public Entity rootNewParent;

    public int newParentChildId;
}