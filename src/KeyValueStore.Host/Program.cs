using KeyValueStore.Application;

namespace KeyValueStore.Host;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var server = new TcpServer();

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            _ = server.StopAsync();
        };

        await server.StartAsync(8080);
    }
}
