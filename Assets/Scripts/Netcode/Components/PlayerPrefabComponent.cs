using Unity.Entities;
using UnityEngine;

public partial struct PlayerPrefabComponent : IComponentData 
{ 
    public Entity prefab;
}