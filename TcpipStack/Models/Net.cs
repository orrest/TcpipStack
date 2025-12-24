namespace TcpipStack.Models;

internal class Net
{
    public required string TopologyName { get; set; }
    ICollection<Device> Devices { get; set; } = [];

    public static Net Create(string topologyName)
    {
        var net = new Net()
        {
            TopologyName = topologyName,
        };

        return net;
    }

    public Device AddDevice(string deviceName)
    {
        var node = new Device()
        {
            Name = deviceName,
        };

        this.Devices.Add(node);

        return node;
    }

    public static void Link(Device one, Device another, string fromInterfaceName, string toInterfaceName, int cost)
    {
        // create network interface for device
        var inter1 = new Interface(
            Name: fromInterfaceName, 
            Owner: one);
        one.AddInterface(inter1);

        var inter2 = new Interface(
            Name: toInterfaceName, 
            Owner: another);
        another.AddInterface(inter2);

        // create a link object
        var link = new Link(
            Interface1: inter1, 
            Interface2: inter2, 
            Cost: cost);

        // add a link between interfaces
        inter1.Connect(link);
        inter2.Connect(link);
    }
}
