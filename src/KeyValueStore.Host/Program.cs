using KeyValueStore.Application;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Diagnostics.Metrics;

namespace KeyValueStore.Host;

public static class Program
{
    public static readonly System.Diagnostics.ActivitySource ActivitySource = new("KeyValueStore.Server");
    public static readonly Meter Meter = new("KeyValueStore.Server");

    public static async Task Main(string[] args)
    {
        // Configure OpenTelemetry TracerProvider
        var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource("KeyValueStore.Server")
            .AddConsoleExporter()
            .Build();

        // Configure OpenTelemetry MeterProvider
        var meterProvider = Sdk.CreateMeterProviderBuilder()
            .AddMeter("KeyValueStore.Server")
            .AddConsoleExporter()
            .Build();

        using var store = new SimpleStore();
        var server = new TcpServer(store, ActivitySource, Meter, maxMessageSize: 4096, maxConcurrentConnections: 100);

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            _ = server.StopAsync();
        };

        await server.StartAsync(8080);

        tracerProvider?.Dispose();
        meterProvider?.Dispose();
    }
}
