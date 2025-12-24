using System.Net;

namespace TcpipStack.Models;

internal class Device
{
    private readonly ICollection<Interface> interfaces = [];

    public required string Name { get; set; }
    public IPAddress LoopBackAddress { get; set; } = null!;

    public void AddInterface(Interface iface)
    {
        this.interfaces.Add(iface);
    }
}
