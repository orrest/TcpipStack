using System.Net.Sockets;
using System.Text;
using TcpipStack.Helpers;
using TcpipStack.Models;

namespace Test;

[TestClass]
public class SendTest
{
    [TestMethod]
    public async Task TestSend()
    {
        Net net = NetHelper.Topo1();

        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        await net.StartAsync(token);

        Device r0 = net.NameDevices["R0_re"];
        Interface? eth00 = r0.GetInterface("eth0/0");
        Assert.IsNotNull(eth00, "Interface eth0/0 not found on R0_re");

        Link? link0001 = eth00.Link;
        Assert.IsNotNull(link0001, "Link not found on interface eth0/0 of R0_re");

        Interface? eth01 = link0001.Interface1 == eth00 ? link0001.Interface2 : link0001.Interface1;
        Assert.IsNotNull(eth01, "Other interface on the link is null");

        Device r1 = eth01.Owner;

        // Send message
        string payload = "Hello, World!";
        byte[] data = Encoding.UTF8.GetBytes(payload);
        await r0.SendAsync(data, r1.AppPort);

        var tcs = new TaskCompletionSource<UdpReceiveResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        r1.MessageReceived += (_, result) => tcs.TrySetResult(result);

        UdpReceiveResult result = await tcs.Task;
        string received = Encoding.UTF8.GetString(result.Buffer);

        Assert.AreEqual(payload, received, "Received payload does not match sent payload");
    }
}
