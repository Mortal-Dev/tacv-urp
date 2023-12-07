using Unity.Entities;

public partial struct NetworkedPrefabComponent : IComponentData
{
    public Entity prefab;
}