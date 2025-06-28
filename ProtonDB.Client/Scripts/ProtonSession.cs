using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace ProtonDB.Client {
    /// <summary>
    /// Represents a low-level session for communicating with a ProtonDB server over TCP.
    /// Handles sending commands and receiving responses in JSON format.
    /// </summary>
    public class ProtonSession : IDisposable {
        private readonly TcpClient _client;
        private readonly StreamReader _reader;
        private readonly StreamWriter _writer;

        /// <summary>
        /// Gets a value indicating whether the session is currently connected to the server.
        /// </summary>
        public bool IsConnected => _client?.Connected ?? false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtonSession"/> class and connects to the specified host and port.
        /// </summary>
        /// <param name="host">The server host address.</param>
        /// <param name="port">The server port.</param>
        public ProtonSession(string host, int port) {
            _client = new TcpClient();
            _client.Connect(host, port);

            var stream = _client.GetStream();
            _reader = new StreamReader(stream, Encoding.UTF8);
            _writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            // Optionally clear any initial server greeting or banner
            if (_reader.Peek() >= 0)
                _reader.ReadLine();
        }

        /// <summary>
        /// Sends a command and optional data to the server asynchronously and returns the deserialized response.
        /// </summary>
        /// <param name="command">The command to send.</param>
        /// <param name="data">Optional data to include with the command.</param>
        /// <returns>A <see cref="ProtonResponse"/> representing the server's response.</returns>
        private async Task<ProtonResponse> SendAsync(string command, string? data = null) {
            var request = new ProtonRequest { Command = command, Data = data };
            string json = JsonSerializer.Serialize(request);
            await _writer.WriteLineAsync(json);

            string? responseLine = await _reader.ReadLineAsync();
            return JsonSerializer.Deserialize<ProtonResponse>(responseLine!)!;
        }

        /// <summary>
        /// Sends a query command to the server asynchronously.
        /// </summary>
        /// <param name="query">The query string to execute.</param>
        /// <returns>A <see cref="ProtonResponse"/> with the query result.</returns>
        public Task<ProtonResponse> QueryAsync(string query) => SendAsync("QUERY", query);

        /// <summary>
        /// Sends a fetch command to retrieve results from the server asynchronously.
        /// </summary>
        /// <returns>A <see cref="ProtonResponse"/> containing the fetched results.</returns>
        public Task<ProtonResponse> FetchAsync() => SendAsync("FETCH");

        /// <summary>
        /// Enables or disables debug mode on the server asynchronously.
        /// </summary>
        /// <param name="enable">True to enable debug mode, false to disable.</param>
        /// <returns>A <see cref="ProtonResponse"/> indicating the result of the debug command.</returns>
        public Task<ProtonResponse> DebugAsync(bool enable) => SendAsync("DEBUG", enable.ToString().ToLower());

        /// <summary>
        /// Sends a quit command to the server asynchronously, closing the session.
        /// </summary>
        /// <returns>A <see cref="ProtonResponse"/> indicating the result of the quit command.</returns>
        public Task<ProtonResponse> QuitAsync() => SendAsync("QUIT");

        /// <summary>
        /// Sends a login command to the server asynchronously with the provided credentials.
        /// </summary>
        /// <param name="userName">The username for authentication.</param>
        /// <param name="password">The password for authentication.</param>
        /// <returns>A <see cref="ProtonResponse"/> indicating the result of the login attempt.</returns>
        public Task<ProtonResponse> LoginAsync(string? userName, string? password) => SendAsync("LOGIN", $"{userName},{password}");

        /// <summary>
        /// Sends a profile command to retrieve results from the server asynchronously.
        /// </summary>
        /// <returns>A <see cref="ProtonResponse"/> containing the results of the profile.</returns>
        public Task<ProtonResponse> ProfileAsync() => SendAsync("PROFILE");

        /// <summary>
        /// Disposes the session, closing the underlying network resources.
        /// </summary>
        public void Dispose() {
            _writer.Dispose();
            _reader.Dispose();
            _client.Close();
        }
    }
}