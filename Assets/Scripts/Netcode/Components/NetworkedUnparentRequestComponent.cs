using Unity.Entities;

public partial struct NetworkedUnparentRequestComponent : IComponentData
{
    public Entity rootParent;
}
