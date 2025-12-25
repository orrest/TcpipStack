using Bogus;
using System.Net;

namespace TcpipStack.Models;

/// <summary>
/// A network interface attached to a device.
/// </summary>
/// <param name="Name"></param>
/// <param name="Owner">The device.</param>
/// <param name="IPAddress">The ip address belongs to interface, and inteface belongs to device.</param>
/// <param name="Mask"></param>
internal record Interface(string Name, Device Owner, IPAddress IPAddress, int Mask)
{
    private Link? link;
    public string MacAddress { get; } = new Faker().Internet.Mac();

    public void AddLink(Link link)
    {
        this.link = link;
    }
}
