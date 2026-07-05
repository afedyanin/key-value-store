using NBomber.CSharp;
using KeyValueStore.Benchmarks;

var benchmarkHost = "127.0.0.1";
var benchmarkPort = 8080;

var scenario = Scenario.Create(
        "key_value_benchmark",
        () => Step.Create(
            "benchmark_step",
            async (_, _) =>
            {
                using var client = new TcpBenchmarkClient(benchmarkHost, benchmarkPort);
                await client.ConnectAsync();

                var key = $"bench-key-{Guid.NewGuid():N}";
                var value = $"bench-value-{Guid.NewGuid():N}";

                var setResult = await client.SetAsync(key, value);
                if (setResult != "OK")
                {
                    return Response.Fail<object>(null, $"SET returned '{setResult}' instead of 'OK'");
                }

                var getResult = await client.GetAsync(key);
                if (getResult is null)
                {
                    return Response.Fail<object>(null, "GET returned null");
                }

                return Response.Ok();
            }))
    .WithWarmUpDuration(TimeSpan.FromSeconds(5))
    .WithLoad(Simulation.Inject(100, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1)))
    .WithDuration(TimeSpan.FromSeconds(30));

await NBomberRunner.RegisterScenarios(scenario)
    .WithReportOptions(reportInterval: TimeSpan.FromSeconds(5))
    .RunAsync();
