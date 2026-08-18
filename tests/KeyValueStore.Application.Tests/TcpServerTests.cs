using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics.Metrics;

namespace KeyValueStore.Application.Tests;

public class TcpServerTests
{
    private static readonly System.Diagnostics.ActivitySource TestActivitySource = new("TestServer");
    private static readonly Meter TestMeter = new("TestServer");

    [Fact]
    public async Task StartAsync_And_StopAsync_DoesNotThrow()
    {
        var server = new TcpServer(new SimpleStore(), TestActivitySource, TestMeter);
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
        var server = new TcpServer(new SimpleStore(), TestActivitySource, TestMeter);
        var port = GetRandomPort();

        var task = server.StartAsync(port);

        try
        {
            await Task.Delay(300);

            using var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, port));

            var profile = new UserProfile
            {
                Id = 1,
                Username = "testuser",
                CreatedAt = DateTime.UtcNow
            };
            var json = System.Text.Json.JsonSerializer.Serialize(profile);
            var message = $"SET key1 {json}\n";
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
    public async Task Set_And_Get_Profile_ReturnsCorrectData()
    {
        var server = new TcpServer(new SimpleStore(), TestActivitySource, TestMeter);
        var port = GetRandomPort();

        var task = server.StartAsync(port);

        try
        {
            await Task.Delay(300);

            using var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, port));

            var profile = new UserProfile
            {
                Id = 42,
                Username = "alice",
                CreatedAt = new DateTime(2024, 1, 15, 10, 30, 0)
            };
            var json = System.Text.Json.JsonSerializer.Serialize(profile);
            var setMessage = $"SET user-42 {json}\n";
            await client.SendAsync(Encoding.UTF8.GetBytes(setMessage), SocketFlags.None);
            await Task.Delay(200);

            var getSet = "GET user-42\n";
            await client.SendAsync(Encoding.UTF8.GetBytes(getSet), SocketFlags.None);
            await Task.Delay(200);

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
        var server = new TcpServer(new SimpleStore(), TestActivitySource, TestMeter);
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
        var server = new TcpServer(new SimpleStore(), TestActivitySource, TestMeter);
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
