using Kisetsu.Utils;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ProtonDB.Server.Core;
using System.Text.Json;

namespace ProtonDB.Server {
    public static class ProtonServer {
        public static async Task Main(string[] args) {
            
            var config = ConfigLoader.Load();
            var server = new QueryServer(config.Port);
            await server.StartAsync();
        }
    }

    public class QuerySession {
        public string? LastQuery { get; set; }
        public string[]? Result { get; set; }
        public bool Debug { get; set; } = false;
        public bool ShouldExit { get; set; } = false;
    }


    public class Request {
        public string? Command { get; set; }
        public string? Data { get; set; }
    }

    public class Response {
        public string Status { get; set; } = "ok";
        public string Message { get; set; } = "";
        public string[]? Result { get; set; }
    }

    public class ServerConfig {
        public string Host { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 9090;
        public bool Debug { get; set; } = false;
        public int MaxConnections { get; set; } = 100;
    }

    public static class ConfigLoader {
        static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions {
            WriteIndented = true
        };

        public static ServerConfig Load(string filePath = "config.json") {
            if (!File.Exists(filePath)) {
                var defaultConfig = new ServerConfig();
                var options = _jsonOptions;
                File.WriteAllText(filePath, JsonSerializer.Serialize(defaultConfig, options));
                Console.WriteLine("Config file not found. Generated default config.");
                return defaultConfig;
            }

            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<ServerConfig>(json)!;
        }
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
                while (!session.ShouldExit && client.Connected) {
                    string? input = await reader.ReadLineAsync();
                    if (input == null) break;
                    var request = JsonSerializer.Deserialize<Request>(input!); 
                    if (request == null) continue;

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
                Console.WriteLine($"[ERROR] {ex.Message}");
            } finally {
                _sessions.TryRemove(client, out _);
                client.Close();
            }
        }
    }
}