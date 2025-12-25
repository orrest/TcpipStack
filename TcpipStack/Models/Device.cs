using System.Net;

namespace TcpipStack.Models;

internal record Device(string Name, IPAddress LoopBackAddress)
{
    /// <summary>
    /// Normally, a device may have only one interface, such as laotop,
    /// but it's alright for a device to have multiple interfaces,
    /// such as server or workstation.
    /// </summary>
    private readonly Dictionary<string, Interface> interfaces = [];

    public void AddInterface(Interface iface)
    {
        if (this.interfaces.ContainsKey(iface.Name))
        {
            throw new ArgumentException($"There's already an interface called {iface.Name}");
        }

        this.interfaces[iface.Name] = iface;
    }
}
