using System.Net;
using System.Net.Sockets;
using TcpipStack.Helpers;

namespace TcpipStack.Models;

public class Device : IAsyncDisposable
{
    /// <summary>
    /// Normally, a device may have only one interface, such as laptop,
    /// but it's alright for a device to have multiple interfaces,
    /// such as a server or workstation.
    /// </summary>
    private readonly Dictionary<string, Interface> interfaces = [];
    
    /// <summary>
    /// Communicate with other Device's app, send and receive.
    /// Listen on IPAddress.Any and self AppPort.
    /// Send to LAN IP address or loopback address and target device's AppPort.
    /// </summary>
    private UdpClient? app;

    /// <summary>
    /// Could be LAN IP address or just loopback address.
    /// </summary>
    private readonly IPAddress communicationTargetAddress;

    private Task? receiveTask;

    private CancellationTokenSource? cts;

    /// <summary>
    /// Simulate an app running in a device.
    /// </summary>
    public int AppPort { get; }
    public string Name { get; }

    public event EventHandler<UdpReceiveResult>? MessageReceived;


    public Device(string name, IPAddress communicationTargetAddress)
    {
        Name = name;

        this.communicationTargetAddress = communicationTargetAddress;

        AppPort = FakerHelper.Faker.Random.Int(2000, 65000);
    }

    public Interface? GetInterface(string name)
    {
        if (this.interfaces.TryGetValue(name, out var iface))
        {
            return iface;
        }

        return null;
    }

    public void AddInterface(Interface iface)
    {
        if (this.interfaces.ContainsKey(iface.Name))
        {
            throw new ArgumentException($"There's already an interface called {iface.Name}");
        }

        Console.WriteLine($"[:{AppPort}] Add interface <{iface.Name}>");

        this.interfaces[iface.Name] = iface;
    }

    /// <summary>
    /// Returns immediately, but leave a background task running to receive messages.
    /// </summary>
    /// <param name="cancellationToken">Use this token to stop the background receiving task.</param>
    /// <returns></returns>
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        app = new UdpClient(new IPEndPoint(this.communicationTargetAddress, AppPort));
        receiveTask = ReceiveLoopAsync(cts.Token);

        return Task.CompletedTask;
    }

    private async Task ReceiveLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            UdpReceiveResult result = await app!.ReceiveAsync(token);

            MessageReceived?.Invoke(this, result);
        }
    }

    public Task<int> SendAsync(byte[] data, int remotePort)
    {
        if (app is null)
        {
            throw new InvalidOperationException("Device not started.");
        }

        Console.WriteLine($"[:{AppPort}] sending message to [:{remotePort}]");

        return app.SendAsync(data, data.Length, new IPEndPoint(IPAddress.Loopback, remotePort));
    }

    public async Task StopAsync()
    {
        cts?.Cancel();
        if (receiveTask is not null)
        {
            try { await receiveTask.ConfigureAwait(false); } 
            catch { }
        }

        app?.Close();
        app?.Dispose();
        app = null;

        cts?.Dispose();
        cts = null;
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(continueOnCapturedContext: false);
    }
}
