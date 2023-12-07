using Riptide;
using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Collections;

public class NetworkSceneManager
{
    public World NetworkWorld => World.DefaultGameObjectInjectionWorld;

    public NetworkedEntityContainer NetworkedEntityContainer { get; private set; }

    public int subsceneCount = 1;

    private string sceneToLoadName;

    public AsyncOperation LoadScene(string sceneName)
    {
        Debug.Log("called load scene in network scene manager");

        AsyncOperation newSceneOperation;

        sceneToLoadName = sceneName;

        SceneFinishedLoadingSystem sceneFinishedLoadingSystem = (SceneFinishedLoadingSystem)World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged(typeof(SceneFinishedLoadingSystem));

        if (NetworkManager.Instance.NetworkType == NetworkType.Host || NetworkManager.Instance.NetworkType == NetworkType.Server)
        {
            newSceneOperation = SceneManager.LoadSceneAsync(sceneName);

            Debug.Log("beginning loading scene server/host");

            newSceneOperation.completed += (AsyncOperation ao) =>
            {
                sceneFinishedLoadingSystem.StartChecking();

                sceneFinishedLoadingSystem.FinishedLoadingSubScenesCompleted += () =>
                {
                    sceneFinishedLoadingSystem.StopChecking();

                    NetworkedEntityContainer?.DestroyAllNetworkedEntities();

                    if (NetworkManager.Instance.NetworkType == NetworkType.Host)
                    {
                        NetworkedEntityContainer = new HostNetworkedEntityContainer(NetworkWorld.EntityManager);
                        NetworkedEntityContainer.SetupSceneNetworkedEntities();

                        HostNetwork hostNetwork = (HostNetwork)NetworkManager.Instance.Network;

                        hostNetwork.Client.Connect("127.0.0.1" + ":" + hostNetwork.Server.Port);
                    }
                    else
                    {
                        NetworkedEntityContainer = new ServerNetworkedEntityContainer(NetworkWorld.EntityManager);
                        NetworkedEntityContainer.SetupSceneNetworkedEntities();

                    }

                    ((FixedStepSimulationSystemGroup)NetworkWorld.GetExistingSystemManaged(typeof(FixedStepSimulationSystemGroup))).Timestep = 1f / NetworkManager.TICKS_PER_SECOND;
                };
            };

            SendServerLoadSceneMessage(sceneName, NetworkManager.SERVER_NET_ID);
        }
        else
        {
            newSceneOperation = SceneManager.LoadSceneAsync(sceneName);

            Debug.Log("beginning loading scene client");

            newSceneOperation.completed += (AsyncOperation asyncOperation) =>
            {
                sceneFinishedLoadingSystem.StartChecking();

                sceneFinishedLoadingSystem.FinishedLoadingSubScenesCompleted += () =>
                {
                    sceneFinishedLoadingSystem.StopChecking();

                    NetworkedEntityContainer?.DestroyAllNetworkedEntities();

                    NetworkedEntityContainer = new ClientNetworkedEntityContainer(NetworkWorld.EntityManager);

                    NetworkedEntityContainer.SetupSceneNetworkedEntities();
                    SendClientCompletedSceneMessage();

                    ((FixedStepSimulationSystemGroup)NetworkWorld.GetExistingSystemManaged(typeof(FixedStepSimulationSystemGroup))).Timestep = 1f / NetworkManager.TICKS_PER_SECOND;
                };
            };
        }

        SetupConnectionEvents();

        return newSceneOperation;
    }

    public AsyncOperation LoadScene(int sceneIndex) => LoadScene(SceneManager.GetSceneByBuildIndex(sceneIndex).name);

    private void SetupConnectionEvents()
    {
        switch (NetworkManager.Instance.NetworkType)
        {
            case NetworkType.Server:
                ((ServerNetwork)NetworkManager.Instance.Network).Server.ClientConnected += (o, e) =>
                {
                    SendServerLoadSceneMessage(sceneToLoadName, e.Client.Id);
                };
                break;
            case NetworkType.Host:
                ((HostNetwork)NetworkManager.Instance.Network).Server.ClientConnected += (o, e) =>
                {
                    SendServerLoadSceneMessage(sceneToLoadName, e.Client.Id);
                };
                break;
        }
    }

    private void SendClientCompletedSceneMessage()
    {
        Message message = Message.Create(MessageSendMode.Reliable, (ushort)ClientToServerNetworkMessageId.ClientFinishedLoadingScene);

        message.Add(sceneToLoadName);

        NetworkManager.Instance.Network.SendMessage(message, SendMode.Client);
    }

    private void SendSpawnNetworkedEntityMessage(int prefabHash, ushort connectionOwnerId, LocalTransform localTransform, ulong networkedEntityId, ushort sendToClientId = NetworkManager.SERVER_NET_ID)
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientNetworkMessageId.ServerSpawnEntity);

        message.Add(prefabHash);
        message.Add(connectionOwnerId);
        message.Add(networkedEntityId);
        message.AddLocalTransform(localTransform);

        NetworkManager.Instance.Network.SendMessage(message, SendMode.Server, sendToClientId);
    }

    private void SendServerLoadSceneMessage(string sceneName, ushort sendToClientId)
    {
        Message message = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClientNetworkMessageId.ServerLoadScene);

        message.AddString(sceneName);

        NetworkManager.Instance.Network.SendMessage(message, SendMode.Server, sendToClientId);
    }

    [MessageHandler((ushort)ClientToServerNetworkMessageId.ClientFinishedLoadingScene)]
    private static void ClientFinishedLoadingScene(ushort clientId, Message message)
    {
        //if we're host, don't do this method
        if (clientId == NetworkManager.CLIENT_NET_ID) return;

        string sceneClientFinishedLoadingName = message.GetString();

        if (!sceneClientFinishedLoadingName.Equals(NetworkManager.Instance.NetworkSceneManager.sceneToLoadName))
        {
            Debug.LogWarning($"client sending completed load scene: {sceneClientFinishedLoadingName}, which is not the current scene: {SceneManager.GetActiveScene().name}");
            return;
        }

        CheckForDestroyedNetworkedSceneEntities(clientId);

        SendEntitySpawns(clientId);
    }

    [MessageHandler((ushort)ServerToClientNetworkMessageId.ServerLoadScene)]
    private static void ServerLoadScene(Message message)
    {
        //if we're host, don't do this method
        if (NetworkManager.Instance.NetworkType == NetworkType.Host) return;

        NetworkManager.Instance.NetworkSceneManager.LoadScene(message.GetString());
    }

    private static void CheckForDestroyedNetworkedSceneEntities(ushort clientId)
    {
        foreach (KeyValuePair<ulong, bool> activeNetworkedSceneEntityPair in NetworkManager.Instance.NetworkSceneManager.NetworkedEntityContainer.SceneEntitiesActive)
        {
            if (activeNetworkedSceneEntityPair.Value) continue;

            Message destroyEntityMessage = Message.Create(MessageSendMode.Reliable, ServerToClientNetworkMessageId.ServerDestroyEntity);

            destroyEntityMessage.AddULong(activeNetworkedSceneEntityPair.Key);

            NetworkManager.Instance.Network.SendMessage(destroyEntityMessage, SendMode.Server, clientId);
        }
    }

    private static void SendEntitySpawns(ushort clientId)
    {
       // IEnumerator<KeyValuePair<ulong, Entity>> enumerator = NetworkManager.Instance.NetworkSceneManager.NetworkedEntityContainer.GetEnumerator();

        foreach (KeyValuePair<ulong, Entity> idEntityPair in NetworkManager.Instance.NetworkSceneManager.NetworkedEntityContainer)
        {
            if (NetworkManager.Instance.NetworkSceneManager.NetworkedEntityContainer.SceneEntitiesActive.ContainsKey(idEntityPair.Key)) continue;

            NetworkedEntityComponent networkedEntityComponent = NetworkManager.Instance.NetworkSceneManager.NetworkWorld.EntityManager.GetComponentData<NetworkedEntityComponent>(idEntityPair.Value);

            LocalTransform localTransform = NetworkManager.Instance.NetworkSceneManager.NetworkWorld.EntityManager.GetComponentData<LocalTransform>(idEntityPair.Value);

            NetworkManager.Instance.NetworkSceneManager.SendSpawnNetworkedEntityMessage(networkedEntityComponent.networkedPrefabHash, networkedEntityComponent.connectionId, localTransform,
                networkedEntityComponent.networkEntityId, clientId);
        }

        /*while (enumerator.MoveNext())
        {
            KeyValuePair<ulong, Entity> idEntityPair = enumerator.Current;

            if (NetworkManager.Instance.NetworkSceneManager.NetworkedEntityContainer.SceneEntitiesActive.ContainsKey(idEntityPair.Key)) continue;

            NetworkedEntityComponent networkedEntityComponent = NetworkManager.Instance.NetworkSceneManager.NetworkWorld.EntityManager.GetComponentData<NetworkedEntityComponent>(idEntityPair.Value);

            LocalTransform localTransform = NetworkManager.Instance.NetworkSceneManager.NetworkWorld.EntityManager.GetComponentData<LocalTransform>(idEntityPair.Value);

            NetworkManager.Instance.NetworkSceneManager.SendSpawnNetworkedEntityMessage(networkedEntityComponent.networkedPrefabHash, networkedEntityComponent.connectionId, localTransform,
                networkedEntityComponent.networkEntityId, clientId);
        }*/
    }
}