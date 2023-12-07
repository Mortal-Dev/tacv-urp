using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Riptide;
using Unity.Entities;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[CreateAfter(typeof(TickSystem))]
[ServerSystem]
public partial class PlayerSpawner : SystemBase
{
    bool addedEvent = false;

    protected override void OnUpdate()
    {
        if (!addedEvent && NetworkManager.Instance.Server != null) 
        {
            NetworkManager.Instance.OnServerClientConnected += OnClientConnected;
            addedEvent = true;
        }
    }

    private void OnClientConnected(ServerConnectedEventArgs serverConnectedEventArgs)
    {
        EntityManager entityManager = NetworkManager.Instance.NetworkSceneManager.NetworkWorld.EntityManager;

        using EntityQuery entityQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<PlayerPrefabComponent>());

        if (!entityQuery.TryGetSingleton(out PlayerPrefabComponent playerPrefabComponent)) return;

        NetworkManager.Instance.NetworkSceneManager.NetworkedEntityContainer.CreateNetworkedEntity(playerPrefabComponent.prefab, serverConnectedEventArgs.Client.Id);
    }
}
