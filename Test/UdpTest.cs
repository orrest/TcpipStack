using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Test;

[TestClass]
public sealed class UdpTest
{
    [TestMethod]
    public async Task SendAndReceive()
    {
        // Local ports
        const int listenPort = 12000;

        // Response message
        var ResponseText = $"Hello from server, {IPAddress.Loopback}:{listenPort}";

        // Cancellation for test lifetime
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));

        // Create a UDP listener bound to loopback:listenPort - this will echo received datagrams back to sender.
        using UdpClient listener = new(new IPEndPoint(IPAddress.Loopback, listenPort));

        Task listenerTask = Task.Run(async () =>
        {
            try
            {
                while (true)
                {
                    UdpReceiveResult received;
                    try
                    {
                        received = await listener.ReceiveAsync();
                    }
                    catch (ObjectDisposedException)
                    {
                        // Listener disposed/closed -> exit loop
                        break;
                    }
                    catch (SocketException)
                    {
                        // Socket closed/errored -> exit loop
                        break;
                    }

                    // Echo the received buffer back to the sender
                    if (received.Buffer != null && received.Buffer.Length > 0)
                    {
                        var resonse = Encoding.ASCII.GetBytes(ResponseText);
                        await listener.SendAsync(resonse, resonse.Length, received.RemoteEndPoint);
                    }
                }
            }
            finally
            {
                // Ensure listener is closed
                listener.Close();
            }
        }, cts.Token);

        // Give the listener a moment to start
        await Task.Delay(100, cts.Token);

        // Create a UdpClient sender that sends to the local listener's port.
        using UdpClient sender = new();
        try
        {
            sender.Connect(IPAddress.Loopback.ToString(), listenPort);

            byte[] sendBytes = Encoding.ASCII.GetBytes("Say hello to server.");
            await sender.SendAsync(sendBytes, sendBytes.Length);

            // Wait for the echo reply
            UdpReceiveResult reply = await sender.ReceiveAsync();
            string replyText = Encoding.ASCII.GetString(reply.Buffer);

            Console.WriteLine("Received from server:");
            Console.WriteLine(replyText);
            Assert.AreEqual(ResponseText, replyText);
        }
        finally
        {
            // Ensure sender is closed
            sender.Close();

            // Stop the listener task
            cts.Cancel();
            try
            {
                // Closing listener will cause ReceiveAsync to throw and end the loop.
                listener.Close();

                await listenerTask;
            }
            catch
            {
                // swallow; test cleanup
            }
        }
    }

    [TestMethod]
    public async Task ListenMultiplePortsAndEcho()
    {
        // Ports to listen on
        int[] ports = { 12001, 12002, 12003 };

        // Cancellation for test lifetime
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));

        // Create listeners bound to loopback for each port
        UdpClient[] listeners = new UdpClient[ports.Length];
        Task[] listenerTasks = new Task[ports.Length];

        for (int i = 0; i < ports.Length; i++)
        {
            int port = ports[i];
            var client = new UdpClient(new IPEndPoint(IPAddress.Loopback, port));
            listeners[i] = client;

            listenerTasks[i] = Task.Run(async () =>
            {
                try
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        UdpReceiveResult received;
                        try
                        {
                            received = await client.ReceiveAsync();
                        }
                        catch (ObjectDisposedException)
                        {
                            break;
                        }
                        catch (SocketException)
                        {
                            break;
                        }

                        if (received.Buffer != null && received.Buffer.Length > 0)
                        {
                            // Build a simple response that includes the listening port
                            string responseText = $"Echo from port {port}";
                            byte[] responseBytes = Encoding.ASCII.GetBytes(responseText);

                            await client.SendAsync(responseBytes, responseBytes.Length, received.RemoteEndPoint);
                        }
                    }
                }
                finally
                {
                    client.Close();
                }
            }, cts.Token);
        }

        // Give listeners a moment to start
        await Task.Delay(100, cts.Token);

        // Test: send to each port and verify we get the correct echo
        using UdpClient sender = new();
        try
        {
            var random = new Random();
            for (int times = 0; times < 10; times++)
            {
                var port = ports[random.Next(0, 3)];
                // Connect the sender to the target listening port, send one message and await reply
                sender.Connect(IPAddress.Loopback.ToString(), port);

                byte[] ping = Encoding.ASCII.GetBytes($"ping:{port}");
                await sender.SendAsync(ping, ping.Length);

                UdpReceiveResult reply = await sender.ReceiveAsync();
                string replyText = Encoding.ASCII.GetString(reply.Buffer);

                Console.WriteLine($"Sent to {port}, received: {replyText}");
                Assert.AreEqual($"Echo from port {port}", replyText);
            }
        }
        finally
        {
            sender.Close();

            // Stop all listeners
            cts.Cancel();
            try
            {
                foreach (var l in listeners)
                {
                    try { l.Close(); } catch { }
                }

                await Task.WhenAll(listenerTasks);
            }
            catch
            {
                // swallow; test cleanup
            }
        }
    }
}
