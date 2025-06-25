using Kisetsu.Utils;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ProtonDB.Server {
    public static class ProtonDB {

        private const int MAX_LENGTH = 128;
        public static string CurrentDatabase { get; set; } = Token.protonDB;

        private static string MultiLineParser(string input, out bool overflow) {
            overflow = false;
            var sb = new StringBuilder();
            input = input.Trim();
            sb.Append(input);

            while (true) {
                if (sb.Length > MAX_LENGTH) {
                    overflow = true;
                    return ("MultiLineParser too long.");
                }

                if (!string.IsNullOrWhiteSpace(input) && input.EndsWith(')')) break;
                input = Terminal.Input($"{ProtonMeta.CurrentDatabase}= ");
                if (string.IsNullOrWhiteSpace(input)) continue;

                sb.Append(input.Trim());
            }
            return sb.ToString().Trim();
        }


        public class QuerySession {
            public string? LastQuery { get; set; }
            public string[]? Result { get; set; }
        }

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
                Console.WriteLine($"ProtonDB server started on port {_port}...");

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
                    while (client.Connected) {
                        string? input = await reader.ReadLineAsync();
                        if (input == null) break;

                        if (input.Trim().ToUpper() == "EXIT") break;

                        if (input.Trim().ToUpper() == "FETCH") {
                            if (session.Result is { Length: > 0 }) {
                                foreach (var line in session.Result)
                                    await writer.WriteLineAsync(line);
                            } else {
                                await writer.WriteLineAsync("No stored result to fetch.");
                            }
                            continue;
                        }

                        session.LastQuery = input;
                        ProtonMeta.Initialize();
                        input = MultiLineParser(input, out bool overflow);
                        if (overflow) {
                            await writer.WriteLineAsync("Input too long. Please try again.");
                            continue;
                        }
                        session.Result = Parser.Execute(input);
                        await writer.WriteLineAsync("Query accepted. Use FETCH to retrieve result.");
                    }
                } catch (Exception ex) {
                    Console.WriteLine($"[ERROR] {ex.Message}");
                } finally {
                    _sessions.TryRemove(client, out _);
                    client.Close();
                }
            }
        }

        public static class Program {
            public static async Task Main(string[] args) {
                var server = new QueryServer(9090);
                await server.StartAsync();
            }
        }
    }
}