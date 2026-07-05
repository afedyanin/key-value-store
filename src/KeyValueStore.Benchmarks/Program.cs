using KeyValueStore.Benchmarks;
using NBomber.CSharp;

var benchmarkHost = "127.0.0.1";
var benchmarkPort = 8080;

var scenario = Scenario.Create(
        "key_value_benchmark",
        async context =>
        {
            try
            {
                using var client = new TcpBenchmarkClient(benchmarkHost, benchmarkPort);
                await client.ConnectAsync();

                var key = $"bench-key-{Guid.NewGuid():N}";
                var value = $"bench-value-{Guid.NewGuid():N}";

                var setResult = await client.SetAsync(key, value);

                if (setResult != "OK")
                {
                    return Response.Fail(message: $"SET returned '{setResult}' instead of 'OK'", statusCode: "500");
                }

                var getResult = await client.GetAsync(key);
                if (getResult == "(nil)")
                {
                    return Response.Fail(message: "GET returned null", statusCode: "500");
                }

                return Response.Ok();
            }
            catch (Exception ex)
            {
                return Response.Fail(message: ex.Message, statusCode: "500");
            }
        })
    .WithWarmUpDuration(TimeSpan.FromSeconds(1))
    .WithLoadSimulations(
        Simulation.Inject(
            rate: 270,
            interval: TimeSpan.FromSeconds(1),
            during: TimeSpan.FromSeconds(30)));

_ = NBomberRunner.RegisterScenarios(scenario).Run();
