using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using KeyValueStore.Application.Abstractions;

namespace KeyValueStore.Application;

public sealed class TcpServer : IDisposable
{
    private readonly IKeyValueStore _store;
    private Socket? _serverSocket;
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isRunning;

    public TcpServer(IKeyValueStore store)
    {
        _store = store;
    }

    public async Task StartAsync(int port = 8080, CancellationToken cancellationToken = default)
    {
        if (_isRunning)
        {
            return;
        }

        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var token = _cancellationTokenSource.Token;

        _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _serverSocket.Bind(new IPEndPoint(IPAddress.Loopback, port));
        _serverSocket.Listen();

        _isRunning = true;

        Console.WriteLine($"TcpServer listening on 127.0.0.1:{port}");

        try
        {
            while (!token.IsCancellationRequested && _isRunning)
            {
                var clientSocket = await _serverSocket.AcceptAsync(token).ConfigureAwait(false);
                _ = ProcessClientAsync(clientSocket, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when stopping
        }
        finally
        {
            _isRunning = false;
        }
    }

    public async Task StopAsync()
    {
        if (!_isRunning)
        {
            return;
        }

        _isRunning = false;
        await _cancellationTokenSource!.CancelAsync().ConfigureAwait(false);

        try
        {
            _serverSocket?.Shutdown(SocketShutdown.Both);
        }
        catch
        {
            // Socket may already be closed
        }
    }

    private async Task ProcessClientAsync(Socket clientSocket, CancellationToken cancellationToken = default)
    {
        var lineBuffer = new StringBuilder();
        var receiveBuffer = ArrayPool<byte>.Shared.Rent(4096);

        Console.WriteLine($"Start processing client socket...");

        try
        {
            while (true)
            {
                int bytesRead;
                try
                {
                    bytesRead = await clientSocket.ReceiveAsync(receiveBuffer.AsMemory(), SocketFlags.None, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    break;
                }

                if (bytesRead == 0)
                {
                    Console.WriteLine("Client disconnected");
                    break;
                }

                var text = Encoding.UTF8.GetString(receiveBuffer.AsSpan(0, bytesRead));
                lineBuffer.Clear();
                lineBuffer.Append(text);

                while (lineBuffer.Length > 0)
                {
                    var span = lineBuffer.ToString().AsSpan();
                    var newlineIndex = span.IndexOf('\n');

                    if (newlineIndex == -1)
                    {
                        break;
                    }

                    var line = span[..newlineIndex].ToString();
                    span = span[(newlineIndex + 1)..];
                    lineBuffer.Clear();
                    lineBuffer.Append(span);

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    //Console.WriteLine($"Line received: {line}");

                    var parsed = CommandParser.Parse(line.AsSpan());

                    if (parsed.Command.IsEmpty)
                    {
                        continue;
                    }

                    //Console.WriteLine($"  Command: {parsed.Command}");
                    //Console.WriteLine($"  Key:     {parsed.Key}");
                    //Console.WriteLine($"  Value:   {parsed.Value}");

                    var response = parsed.Command switch
                    {
                        "SET" => HandleSet(parsed),
                        "GET" => HandleGet(parsed),
                        "DELETE" => HandleDelete(parsed),
                        _ => Encoding.UTF8.GetBytes("-ERR Unknown command\r\n")
                    };

                    if (response != null)
                    {
                        try
                        {
                            await clientSocket.SendAsync(response, SocketFlags.None).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error: {ex.Message}");
                            break;
                        }
                    }
                }
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(receiveBuffer);

            try
            {
                clientSocket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            clientSocket.Close();
        }
    }

    private byte[]? HandleSet(ParsedCommand parsed)
    {
        var key = parsed.Key.ToString();
        var valueSpan = parsed.Value.TrimEnd(['\r', '\n']);
        var profile = JsonSerializer.Deserialize<UserProfile>(valueSpan);

        if (profile == null)
        {
            return Encoding.UTF8.GetBytes("-ERR Failed to deserialize profile\r\n");
        }

        _store.Set(key, profile);
        return Encoding.UTF8.GetBytes("OK\r\n");
    }

    private byte[]? HandleGet(ParsedCommand parsed)
    {
        var key = parsed.Key.ToString();
        var profile = _store.Get(key);

        return profile is not null
            ? Encoding.UTF8.GetBytes(JsonSerializer.Serialize(profile) + "\r\n")
            : Encoding.UTF8.GetBytes("(nil)\r\n");
    }

    private byte[]? HandleDelete(ParsedCommand parsed)
    {
        var key = parsed.Key.ToString();
        _store.Delete(key);
        return Encoding.UTF8.GetBytes("OK\r\n");
    }

    public void Dispose()
    {
        _serverSocket?.Dispose();
        _cancellationTokenSource?.Dispose();
    }
}
