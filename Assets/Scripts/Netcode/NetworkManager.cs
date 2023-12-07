using Riptide.Utils;
using Riptide;
using UnityEngine;
using System;

public class NetworkManager
{
    //will only ever be one instance of NetworkManager
    public static NetworkManager Instance = new NetworkManager();

    public const ushort SERVER_NET_ID = ushort.MaxValue;

    public static ushort CLIENT_NET_ID { get; private set; }

    public static float TICKS_PER_SECOND { get; private set; }

    public NetworkType NetworkType { get; private set; } = NetworkType.None;

    public NetworkSceneManager NetworkSceneManager { get; private set; }

    public delegate void ClientConnected(ClientConnectedEventArgs clientConnectedEventArgs);

    public event ClientConnected OnClientConnected;

    public delegate void ServerClientConnected(ServerConnectedEventArgs serverDisconnectedEventArgs);

    public event ServerClientConnected OnServerClientConnected;

    public delegate void ClientDisconnected(ClientDisconnectedEventArgs clientDisconnectedEventArgs);

    public event ClientDisconnected OnClientDisconnected;

    public delegate void ServerClientDisconnected(ServerDisconnectedEventArgs serverConnectedEventArgs);

    public event ServerClientDisconnected OnServerClientDisconnected;

    public INetwork Network { get; private set; }

    public Server Server
    {
        get
        {
            if (NetworkType == NetworkType.Host) return ((HostNetwork)Network).Server;
            else if (NetworkType == NetworkType.Server) return ((ServerNetwork)Network).Server;

            return null;
        }
    }

    public Client Client
    {
        get
        {
            if (NetworkType == NetworkType.Client) return ((ClientNetwork)Network).Client;

            return null;
        }
    }

    public void Tick() => Network?.Tick();
    
    public NetworkManager()
    {            
        NetworkSceneManager = new NetworkSceneManager();

        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);
    }

    public void StartClient(string connection, string sceneName, float ticksPerSecond = 30)
    {
        TICKS_PER_SECOND = ticksPerSecond;

        NetworkType = NetworkType.Client;

        Network = new ClientNetwork();
        Network.Start(new string[] { connection });

        NetworkSceneManager.LoadScene(sceneName);

        ((ClientNetwork)Network).Client.Connected += (s, e) => CLIENT_NET_ID = ((ClientNetwork)Network).Client.Id;

        ((ClientNetwork)Network).Client.Connected += (s, e) => OnClientConnected?.Invoke((ClientConnectedEventArgs)e);
        ((ClientNetwork)Network).Client.Connected += (s, e) => OnClientDisconnected?.Invoke((ClientDisconnectedEventArgs)e);
    }

    public void StartServer(ushort port, ushort maxPlayerCount, string sceneName, float ticksPerSecond = 30)
    {
        TICKS_PER_SECOND = ticksPerSecond;

        NetworkType = NetworkType.Server;

        Network = new ServerNetwork();
        Network.Start(new string[] { port.ToString(), maxPlayerCount.ToString() });

        NetworkSceneManager.LoadScene(sceneName);

        ((ServerNetwork)Network).Server.ClientConnected += (s, e) => OnServerClientConnected?.Invoke(e);
        ((ServerNetwork)Network).Server.ClientDisconnected += (s, e) => OnServerClientDisconnected?.Invoke(e);
    }

    public void StartHost(ushort port, ushort maxPlayerCount, string sceneName, float ticksPerSecond = 30)
    {
        TICKS_PER_SECOND = ticksPerSecond;

        NetworkType = NetworkType.Host;

        Network = new HostNetwork();
        Network.Start(new string[] { port.ToString(), maxPlayerCount.ToString() });

        NetworkSceneManager.LoadScene(sceneName);

        ((HostNetwork)Network).Client.Connected += (s, e) => CLIENT_NET_ID = ((HostNetwork)Network).Client.Id;

        ((HostNetwork)Network).Client.Connected += (s, e) => OnClientConnected?.Invoke((ClientConnectedEventArgs)e);
        ((HostNetwork)Network).Client.Connected += (s, e) => OnClientDisconnected?.Invoke((ClientDisconnectedEventArgs)e);

        ((HostNetwork)Network).Server.ClientConnected += (s, e) => OnServerClientConnected?.Invoke(e);
        ((HostNetwork)Network).Server.ClientDisconnected += (s, e) => OnServerClientDisconnected?.Invoke(e);
    }

    public void Stop()
    {
        Network?.Stop();
        Network = null;
        CLIENT_NET_ID = 0;
        NetworkSceneManager = new NetworkSceneManager();
    }
}