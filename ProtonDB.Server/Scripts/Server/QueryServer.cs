// -------------------------------------------------------------------------------------------------
//  File: QueryServer.cs
//  Namespace: ProtonDB.Server
//  Description:
//      Implements a TCP server for ProtonDB that listens for client connections, manages
//      user sessions, and processes incoming commands. Each client connection is handled
//      asynchronously, with session state tracked in a thread-safe dictionary. The server
//      enforces authentication, routes commands to their handlers, and provides structured
//      responses. Errors and session events are logged using Meta utilities.
//
//  Public Methods:
//      - QueryServer(int port): Initializes the server to listen on the specified port.
//      - StartAsync(): Starts the server, accepts incoming TCP clients, and handles each
//        connection in a separate task.
//
//  Internal Methods:
//      - HandleClientAsync(TcpClient client): Manages the lifecycle of a client connection,
//        including authentication, command dispatch, and session cleanup.
//
//  Usage Example:
//      var server = new QueryServer(9000);
//      await server.StartAsync();
//
//  Dependencies:
//      - QuerySession: Tracks per-client session state (authentication, profile, results, etc.).
//      - Request: Represents incoming client commands and arguments (deserialized from JSON).
//      - Response: Standardized response object for client communication.
//      - CommandRegistry: Resolves command keywords to IServerCommand handlers.
//      - Meta: Provides logging and initialization utilities.
//      - System.Net, System.Net.Sockets, System.Text.Json: Networking and serialization.
// -------------------------------------------------------------------------------------------------

using ProtonDB.Server.Core;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace ProtonDB.Server {
    /// <summary>
    /// TCP server for ProtonDB that manages client connections, user sessions, and command processing.
    /// </summary>
    public class QueryServer {
        private readonly int _port;
        private readonly TcpListener _listener;
        private readonly ConcurrentDictionary<TcpClient, QuerySession> _sessions = new();

        /// <summary>
        /// Initializes the server to listen on the specified port.
        /// </summary>
        /// <param name="port">The TCP port to listen on.</param>
        public QueryServer(int port) {
            _port = port;
            _listener = new TcpListener(IPAddress.Any, _port);
        }

        /// <summary>
        /// Starts the server, accepts incoming TCP clients, and handles each connection asynchronously.
        /// </summary>
        public async Task StartAsync() {
            _listener.Start();
            Meta.Log($"ProtonDB server started on port {_port}...");

            while (true) {
                var client = await _listener.AcceptTcpClientAsync();
                _ = Task.Run(() => HandleClientAsync(client));
            }
        }

        /// <summary>
        /// Manages the lifecycle of a client connection, including authentication, command dispatch,
        /// and session cleanup. Each client is handled in a separate task.
        /// </summary>
        /// <param name="client">The connected TCP client.</param>
        private async Task HandleClientAsync(TcpClient client) {
            var session = new QuerySession();
            _sessions[client] = session;
            Meta.Initialize(session);
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