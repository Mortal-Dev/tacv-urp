using UnityEngine;
using Unity.Entities;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct NetorkedManagerEntityControllerSystem : ISystem
{
    public void OnUpdate(ref SystemState systemState)
    {
        if (SystemAPI.TryGetSingletonRW(out RefRW<NetworkManagerEntityComponent> networkManagerEntityComponent))
        {
            networkManagerEntityComponent.ValueRW.NetworkType = NetworkManager.Instance.NetworkType;
            networkManagerEntityComponent.ValueRW.localNetworkId = (NetworkManager.Instance.NetworkType == NetworkType.Server) ? NetworkManager.CLIENT_NET_ID : NetworkManager.SERVER_NET_ID;
        }
        else
        {
            Entity networkManagerEntity = systemState.EntityManager.CreateEntity();
            systemState.EntityManager.AddComponentData(networkManagerEntity, new NetworkManagerEntityComponent() { NetworkType = NetworkManager.Instance.NetworkType, 
                localNetworkId = (NetworkManager.Instance.NetworkType == NetworkType.Server) ? NetworkManager.CLIENT_NET_ID : NetworkManager.SERVER_NET_ID });
        }
    }
}