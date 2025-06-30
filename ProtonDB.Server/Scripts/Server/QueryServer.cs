using ProtonDB.Server.Core;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace ProtonDB.Server {
    public class QueryServer {
        private readonly int _port;
        private readonly TcpListener _listener;
        private readonly ConcurrentDictionary<TcpClient, QuerySession> _sessions = new();

        public QueryServer(int port) {
            _port = port;
            _listener = new TcpListener(IPAddress.Any, _port);
        }

        public async Task StartAsync() {
            _listener.Start();
            Meta.Log($"ProtonDB server started on port {_port}...");

            while (true) {
                var client = await _listener.AcceptTcpClientAsync();
                _ = Task.Run(() => HandleClientAsync(client));
            }
        }

        private async Task HandleClientAsync(TcpClient client) {
            var session = new QuerySession();
            _sessions[client] = session;

            using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8);
            using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            await writer.WriteLineAsync("Connected to ProtonDB. Send a query or use FETCH.");

            try {
                while (!session.ShouldExit && client.Connected) {
                    string? input = await reader.ReadLineAsync();
                    if (input == null) break;
                    var request = JsonSerializer.Deserialize<Request>(input!);
                    if (request == null) continue;
                    if (!session.IsAuthenticated && request.Command?.ToUpperInvariant() != Command.login) {
                        await writer.WriteLineAsync(JsonSerializer.Serialize(new Response {
                            Status = "error",
                            Message = "You must log in before issuing other commands."
                        }));
                        continue;
                    }

                    var command = CommandRegistry.Resolve(request.Command?.ToUpperInvariant()!);
                    if (command != null) {
                        await command.ExecuteAsync(session, writer, request);
                        continue;
                    }

                    await writer.WriteLineAsync(JsonSerializer.Serialize(new Response {
                        Status = "error",
                        Message = "Unknown command"
                    }));
                }
            } catch (Exception ex) {
                Meta.Log($"[ERROR] {ex.Message}", session);
            } finally {
                _sessions.TryRemove(client, out _);
                client.Close();
            }
        }
    }
}