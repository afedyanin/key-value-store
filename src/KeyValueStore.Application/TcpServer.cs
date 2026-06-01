using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace KeyValueStore.Application;

public sealed class TcpServer : IDisposable
{
    private Socket? _serverSocket;
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isRunning;

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
                _ = ProcessClientAsync(clientSocket);
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

    private async Task ProcessClientAsync(Socket clientSocket)
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
                    bytesRead = await clientSocket.ReceiveAsync(receiveBuffer.AsMemory(), SocketFlags.None).ConfigureAwait(false);
                }
                catch
                {
                    break;
                }

                if (bytesRead == 0)
                {
                    Console.WriteLine("Client disconnected");
                    break;
                }

                var text = Encoding.UTF8.GetString(receiveBuffer.AsSpan(0, bytesRead));
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

                    // Console.WriteLine($"Line received: {line}");

                    var parsed = CommandParser.Parse(line.AsSpan());

                    if (parsed.Command.IsEmpty)
                    {
                        continue;
                    }

                    Console.WriteLine($"  Command: {parsed.Command}");
                    Console.WriteLine($"  Key:     {parsed.Key}");
                    Console.WriteLine($"  Value:   {parsed.Value}");
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
            catch
            {
                // Socket may already be closed
            }

            clientSocket.Close();
        }
    }

    public void Dispose()
    {
        _serverSocket?.Dispose();
        _cancellationTokenSource?.Dispose();
    }
}
