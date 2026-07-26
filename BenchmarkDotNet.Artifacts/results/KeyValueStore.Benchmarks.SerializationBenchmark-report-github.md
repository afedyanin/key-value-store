```

BenchmarkDotNet v0.15.6, Windows 10 (10.0.19045.6456/22H2/2022Update)
AMD Ryzen 7 7700 3.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.302
  [Host]     : .NET 10.0.10 (10.0.10, 10.0.1026.32716), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 10.0.10 (10.0.10, 10.0.1026.32716), X64 RyuJIT x86-64-v4


```
| Method                                 | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------------------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| Serialize_JsonSerializer               | 114.21 ns | 2.159 ns | 2.120 ns |  1.00 |    0.03 |      - |         - |          NA |
| Serialize_BinaryGenerated              |  21.32 ns | 0.150 ns | 0.140 ns |  0.19 |    0.00 | 0.0091 |     152 B |          NA |
| Deserialize_JsonSerializer             | 331.84 ns | 1.921 ns | 1.604 ns |  2.91 |    0.05 | 0.0067 |     112 B |          NA |
| Deserialize_BinaryGenerated            |  47.01 ns | 0.199 ns | 0.186 ns |  0.41 |    0.01 | 0.0249 |     416 B |          NA |
| Serialize_BinaryGenerated_RentedBuffer |  28.22 ns | 0.159 ns | 0.141 ns |  0.25 |    0.00 |      - |         - |          NA |
