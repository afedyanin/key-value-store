Console.WriteLine("=== Binary Serializer vs System.Text.Json Benchmark ===");
Console.WriteLine();

BenchmarkDotNet.Running.BenchmarkRunner.Run<KeyValueStore.Benchmarks.SerializationBenchmark>();
