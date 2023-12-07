using UnityEngine;
using Riptide;

public class ClientNetwork : INetwork
{
    public Client Client { get; private set; }

    public void Start(string[] args)
    {
        if (args.Length > 1 || args.Length == 0) Debug.LogError("innapropriate size of args for ClientNetwork.Start");

        Client = new Client();
        Client.Connect(args[0]);
    }

    public void Tick()
    {
        if (Client == null)
        {
            Debug.LogError($"must call {nameof(Start)} before using {nameof(Tick)}");
            return;
        }

        Client.Update();
    }

    public void Stop()
    {
        Client.Disconnect();
        Client = null;
    }

    public void SendMessage(Message message, SendMode sendMode = SendMode.Client, ushort sendTo = ushort.MaxValue, bool shouldRelease = true)
    {
        if (sendMode != SendMode.Client)
        {
            Debug.LogError($"{nameof(sendMode)} must be set to {nameof(SendMode.Client)}");
            return;
        }

        Client.Send(message, shouldRelease);
    }
}

