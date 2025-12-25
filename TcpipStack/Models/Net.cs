namespace TcpipStack.Models;

internal record Net(string TopologyName)
{
    public Dictionary<string, Device> Devices { get; set; } = [];

    public void AddDevice(Device device)
    {
        if (this.Devices.ContainsKey(device.Name))
        {
            throw new ArgumentException($"There's already a device named {device.Name}.");
        }

        this.Devices[device.Name] = device;
    }
}
