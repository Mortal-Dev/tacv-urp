using UnityEngine;
using Riptide;

public class ServerNetwork : INetwork
{
    public Server Server { get; private set; }
    
    public void Start(string[] args)
    {
        Server = new Server();
        Server.RelayFilter = new MessageRelayFilter(typeof(ServerToClientNetworkMessageId));
        Server.Start(ushort.Parse(args[0]), ushort.Parse(args[1]));
    }
    
    public void Tick()
    {
        if (Server == null)
        {
            Debug.LogError($"must call {nameof(Start)} before using {nameof(Tick)}");
            return;
        }

        Server.Update();
    }

    public void Stop()
    {
        Server.Stop();
        Server = null;
    }

    public void SendMessage(Message message, SendMode sendMode = SendMode.Server, ushort sendTo = ushort.MaxValue, bool shouldRelease = true)
    {
        if (sendMode != SendMode.Server)
        {
            Debug.LogError($"{nameof(sendMode)} must be set to {nameof(SendMode.Server)}");
            return;
        }

        if (sendTo == ushort.MaxValue)
        {
            Server.SendToAll(message, shouldRelease);
        }
        else
        {
            Server.Send(message, sendTo, shouldRelease);
        }
    }
}