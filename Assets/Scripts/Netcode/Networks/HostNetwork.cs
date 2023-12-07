using UnityEngine;
using Riptide;
using UnityEngine.SceneManagement;
using Unity.Entities;
using System.Reflection;

public class HostNetwork : INetwork
{
    public Server Server { get; private set; }

    public Client Client { get; private set; }

    public void Start(string[] args)
    {
        Server = new Server();
        Server.RelayFilter = new MessageRelayFilter(typeof(ClientToServerNetworkMessageId));
        Server.Start(ushort.Parse(args[0]), ushort.Parse(args[1]));

        Client = new Client();
    }

    public void Tick()
    {
        if (Server == null || Client == null)
        {
            Debug.LogError($"must call {nameof(Start)} before using {nameof(Tick)}");
            return;
        }

        Server.Update();
        Client.Update();
    }

    public void Stop()
    {
        Client.Disconnect();
        Server.Stop();

        Client = null;
        Server = null;
    }

    public void SendMessage(Message message, SendMode sendMode, ushort sendTo = ushort.MaxValue, bool shouldRelease = true)
    {
        if (sendMode == SendMode.Client)
        {
            Client.Send(message, shouldRelease);
            return;
        }
        else if (sendTo == ushort.MaxValue)
        {
            Server.SendToAll(message, shouldRelease);
        }
        else
        {
            Server.Send(message, sendTo, shouldRelease);
        }
    }
}

public enum SendMode
{
    Client,
    Server
}