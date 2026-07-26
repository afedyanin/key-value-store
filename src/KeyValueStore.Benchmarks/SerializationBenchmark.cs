using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using System.Buffers;
using System.Text.Json;
using KeyValueStore.Application;

namespace KeyValueStore.Benchmarks;

[RPlotExporter]
[MemoryDiagnoser]
public class SerializationBenchmark : IDisposable
{
    private UserProfile _profile = null!;
    private readonly MemoryStream _binaryStream = new(256);
    private readonly MemoryStream _jsonStream = new(256);
    private byte[] _buffer = null!;

    [GlobalSetup]
    public void Setup()
    {
        _profile = new UserProfile
        {
            Id = 42,
            Username = "alexey-volobuyev-expert",
            CreatedAt = new DateTime(2026, 7, 24, 12, 0, 0, DateTimeKind.Utc)
        };

        _buffer = ArrayPool<byte>.Shared.Rent(1024);
    }

    [Benchmark(Baseline = true)]
    public void Serialize_JsonSerializer()
    {
        _jsonStream.SetLength(0);
        _jsonStream.Position = 0;
        JsonSerializer.Serialize(_jsonStream, _profile, typeof(UserProfile));
    }

    [Benchmark]
    public void Serialize_BinaryGenerated()
    {
        _binaryStream.SetLength(0);
        _binaryStream.Position = 0;
        _profile.SerializeToBinary(_binaryStream);
    }

    [Benchmark]
    public UserProfile Deserialize_JsonSerializer()
    {
        _jsonStream.SetLength(0);
        _jsonStream.Position = 0;
        JsonSerializer.Serialize(_jsonStream, _profile, typeof(UserProfile));
        _jsonStream.Position = 0;
        return JsonSerializer.Deserialize<UserProfile>(_jsonStream)!;
    }

    [Benchmark]
    public UserProfile Deserialize_BinaryGenerated()
    {
        _binaryStream.SetLength(0);
        _binaryStream.Position = 0;
        _profile.SerializeToBinary(_binaryStream);
        _binaryStream.Position = 0;
        return UserProfile.DeserializeFromBinary(_binaryStream)!;
    }

    [Benchmark]
    public void Serialize_BinaryGenerated_RentedBuffer()
    {
        _binaryStream.SetLength(0);
        _binaryStream.Position = 0;
        var rented = ArrayPool<byte>.Shared.Rent(1024);
        var span = rented.AsSpan();

        WriteInt32(ref span, _profile.Id);
        WriteString(ref span, _profile.Username);
        WriteInt64(ref span, _profile.CreatedAt.ToBinary());

        _binaryStream.Write(rented.AsSpan(0, rented.Length - span.Length));
        ArrayPool<byte>.Shared.Return(rented);
    }

    private static Span<byte> WriteInt32(ref Span<byte> span, int value)
    {
        System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(span, value);
        span = span[sizeof(int)..];
        return span;
    }

    private static Span<byte> WriteInt64(ref Span<byte> span, long value)
    {
        System.Buffers.Binary.BinaryPrimitives.WriteInt64BigEndian(span, value);
        span = span[sizeof(long)..];
        return span;
    }

    private static Span<byte> WriteString(ref Span<byte> span, string? value)
    {
        if (value is null)
        {
            System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(span, -1);
            span = span[sizeof(int)..];
            return span;
        }

        var encoded = System.Text.Encoding.UTF8.GetByteCount(value);
        System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(span, encoded);
        span = span[sizeof(int)..];
        System.Text.Encoding.UTF8.GetBytes(value, span);
        span = span[encoded..];
        return span;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            ArrayPool<byte>.Shared.Return(_buffer);
            _binaryStream.Dispose();
            _jsonStream.Dispose();
        }
    }
}
