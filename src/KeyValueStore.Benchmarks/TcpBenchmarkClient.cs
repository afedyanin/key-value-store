using System.Net.Sockets;
using System.Text;

namespace KeyValueStore.Benchmarks;

public sealed class TcpBenchmarkClient : IDisposable
{
    private Socket? _socket;
    private NetworkStream? _stream;
    private StreamReader? _reader;
    private readonly string _host = "";
    private readonly int _port;
    private bool _disposed;

    public TcpBenchmarkClient(string host, int port)
    {
        _host = host;
        _port = port;
    }

    public async Task ConnectAsync()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        await _socket.ConnectAsync(_host, _port).ConfigureAwait(false);
        _stream = new NetworkStream(_socket);
        _reader = new StreamReader(_stream, Encoding.UTF8, leaveOpen: true);
    }

    public async Task<string> SetAsync(string key, string value)
    {
        var command = $"SET {key} {value}\n";
        var bytes = Encoding.UTF8.GetBytes(command);
        await _stream!.WriteAsync(bytes.AsMemory()).ConfigureAwait(false);
        await _stream!.FlushAsync().ConfigureAwait(false);
        return await ReadResponseAsync().ConfigureAwait(false);
    }

    public async Task<string> SetAsync(string key, byte[] value)
    {
        var valueStr = Encoding.UTF8.GetString(value);
        var command = $"SET {key} {valueStr}\n";
        var bytes = Encoding.UTF8.GetBytes(command);
        await _stream!.WriteAsync(bytes.AsMemory()).ConfigureAwait(false);
        await _stream!.FlushAsync().ConfigureAwait(false);
        return await ReadResponseAsync().ConfigureAwait(false);
    }

    public async Task<string?> GetAsync(string key)
    {
        var command = $"GET {key}\n";
        var bytes = Encoding.UTF8.GetBytes(command);
        await _stream!.WriteAsync(bytes.AsMemory()).ConfigureAwait(false);
        await _stream!.FlushAsync().ConfigureAwait(false);
        return await ReadResponseAsync().ConfigureAwait(false);
    }

    public async Task<string> DeleteAsync(string key)
    {
        var command = $"DELETE {key}\n";
        var bytes = Encoding.UTF8.GetBytes(command);
        await _stream!.WriteAsync(bytes.AsMemory()).ConfigureAwait(false);
        await _stream!.FlushAsync().ConfigureAwait(false);
        return await ReadResponseAsync().ConfigureAwait(false);
    }

    private async Task<string> ReadResponseAsync()
    {
        var sb = new StringBuilder();
        var buffer = new byte[1];
        while ((await _stream!.ReadAsync(buffer).ConfigureAwait(false)) > 0)
        {
            var c = (char)buffer[0];
            sb.Append(c);
            if (c == '\n')
            {
                break;
            }
        }

        var result = sb.ToString().TrimEnd('\r', '\n');
        return result;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        try
        {
            _stream?.Flush();
        }
        catch
        {
            // Ignore
        }

        try
        {
            _socket?.Shutdown(SocketShutdown.Both);
        }
        catch
        {
            // Ignore
        }

        _socket?.Dispose();
        _stream?.Dispose();
        _reader?.Dispose();
    }
}
