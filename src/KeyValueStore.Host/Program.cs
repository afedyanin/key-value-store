using KeyValueStore.Application;

namespace KeyValueStore.Host;

public static class Program
{
    public static async Task Main(string[] args)
    {
        using var store = new SimpleStore();
        var server = new TcpServer(store);

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            _ = server.StopAsync();
        };

        await server.StartAsync(8080);
    }
}
