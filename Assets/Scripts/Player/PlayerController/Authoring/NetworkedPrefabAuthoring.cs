using System.Collections.Generic;
using Unity.Entities;
using Unity.Collections;
using UnityEngine;

public class NetworkedPrefabAuthoring : MonoBehaviour
{
    public GameObject prefab;

    class Baking : Baker<NetworkedPrefabAuthoring>
    {
        public override void Bake(NetworkedPrefabAuthoring authoring)
        {
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), new NetworkedPrefabComponent() { prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic) });
        }
    }
}