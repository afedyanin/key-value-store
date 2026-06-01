using System.Net;
using System.Net.Sockets;
using System.Text;

namespace KeyValueStore.Application.Tests;

public class TcpServerTests
{
    [Fact]
    public async Task StartAsync_And_StopAsync_DoesNotThrow()
    {
        var server = new TcpServer();
        var port = GetRandomPort();

        var task = server.StartAsync(port);

        try
        {
            await Task.Delay(200);
            Assert.True(task.IsCompleted == false, "Server should not be completed after StartAsync");
        }
        finally
        {
            await server.StopAsync();
            await task;
        }
    }

    [Fact]
    public async Task Client_Can_Connect_And_Send_Data()
    {
        var server = new TcpServer();
        var port = GetRandomPort();

        var task = server.StartAsync(port);

        try
        {
            await Task.Delay(300);

            using var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, port));

            var message = "SET key1 value1\n";
            var bytes = Encoding.UTF8.GetBytes(message);
            await client.SendAsync(bytes, SocketFlags.None);

            await Task.Delay(300);

            client.Shutdown(SocketShutdown.Send);
            await Task.Delay(200);
        }
        finally
        {
            await server.StopAsync();
            await task;
        }
    }

    [Fact]
    public async Task Client_Disconnect_Correctly_Handled()
    {
        var server = new TcpServer();
        var port = GetRandomPort();

        var task = server.StartAsync(port);

        try
        {
            await Task.Delay(300);

            using var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, port));

            client.Shutdown(SocketShutdown.Both);

            await Task.Delay(300);
        }
        finally
        {
            await server.StopAsync();
            await task;
        }
    }

    [Fact]
    public async Task Multiple_Clients_Can_Connect()
    {
        var server = new TcpServer();
        var port = GetRandomPort();

        var task = server.StartAsync(port);

        try
        {
            await Task.Delay(300);

            var clients = new List<Socket>();

            for (var i = 0; i < 3; i++)
            {
                var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, port));
                clients.Add(client);
            }

            await Task.Delay(300);

            foreach (var client in clients)
            {
                client.Shutdown(SocketShutdown.Both);
            }

            await Task.Delay(200);
        }
        finally
        {
            await server.StopAsync();
            await task;
        }
    }

    private static int GetRandomPort()
    {
        using var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        var port = ((IPEndPoint)listener.LocalEndPoint!).Port;
        listener.Close();
        return port;
    }
}
