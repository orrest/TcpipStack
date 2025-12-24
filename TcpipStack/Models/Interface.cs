using System.Net;

namespace TcpipStack.Models;

internal record Interface(string Name, Device Owner)
{
    // TODO 一个 interface 只能有一个 link 吗？
    private Link? link;

    public required MacAddress MacAddress { get; set; }
    public IPAddress? IpAddress { get; set; }
    public required char Mask { get; set; }

    public void Connect(Link link)
    {
        this.link = link;
    }
}
