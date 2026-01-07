namespace TcpipStack.Models;

public class Net : IAsyncDisposable
{
    public Dictionary<string, Device> NameDevices { get; set; } = [];
    public string TopologyName { get; }

    public Net(string topologyName)
    {
        TopologyName = topologyName;
    }

    public void AddDevice(Device device)
    {
        if (this.NameDevices.ContainsKey(device.Name))
        {
            throw new ArgumentException($"There's already a device named {device.Name}.");
        }

        this.NameDevices[device.Name] = device;
    }

    public async Task StartAsync(CancellationToken token)
    {
        foreach (var device in this.NameDevices.Values)
        {
            await device.StartAsync(token);
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var device in this.NameDevices.Values)
        {
            await device.DisposeAsync();
        }
    }
}
