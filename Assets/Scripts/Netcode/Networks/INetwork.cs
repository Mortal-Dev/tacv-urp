using Riptide;

public interface INetwork
{
    public void Start(string[] args);

    public void Tick();

    public void Stop();

    public void SendMessage(Message message, SendMode sendMode, ushort sendTo = ushort.MaxValue, bool shouldRelease = true);
}