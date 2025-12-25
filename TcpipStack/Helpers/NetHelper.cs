using System.Net;
using TcpipStack.Models;

namespace TcpipStack.Helpers;

internal class NetHelper
{
    public static Net Topo1()
    {
        var net = new Net("Hello World Generic Graph");

        // add devices to network
        Device r0 = new("R0_re", IPAddress.Parse("122.1.1.0"));
        Device r1 = new("R1_re", IPAddress.Parse("122.1.1.1"));
        Device r2 = new("R2_re", IPAddress.Parse("122.1.1.2"));
        net.AddDevice(r0);
        net.AddDevice(r1);
        net.AddDevice(r2);

        // add interfaces to devices
        var eth00 = new Interface("eth0/0", r0, IPAddress.Parse("20.1.1.1"), 24);
        var eth04 = new Interface("eth0/4", r0, IPAddress.Parse("40.1.1.1"), 24);
        r0.AddInterface(eth00);
        r0.AddInterface(eth04);

        var eth01 = new Interface("eth0/1", r1, IPAddress.Parse("20.1.1.2"), 24);
        var eth02 = new Interface("eth0/2", r1, IPAddress.Parse("30.1.1.1"), 24);
        r1.AddInterface(eth01);
        r1.AddInterface(eth02);

        var eth03 = new Interface("eth0/3", r2, IPAddress.Parse("30.1.1.2"), 24);
        var eth05 = new Interface("eth0/5", r2, IPAddress.Parse("40.1.1.2"), 24);
        r2.AddInterface(eth03);
        r2.AddInterface(eth05);

        // create links between interfaces
        var link_00_01 = new Link(eth00, eth01, 1);
        var link_02_03 = new Link(eth02, eth03, 1);
        var link_04_05 = new Link(eth04, eth05, 1);

        eth00.AddLink(link_00_01);
        eth01.AddLink(link_00_01);

        eth02.AddLink(link_02_03);
        eth03.AddLink(link_02_03);

        eth04.AddLink(link_04_05);
        eth05.AddLink(link_04_05);

        return net;
    }
}
