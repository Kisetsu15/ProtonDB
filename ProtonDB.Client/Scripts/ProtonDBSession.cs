using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace ProtonDB.Client {

    public class ProtonDBSession : IDisposable {
        private readonly TcpClient _client;
        private readonly StreamReader _reader;
        private readonly StreamWriter _writer;
        public bool IsConnected => _client?.Connected ?? false;

        public ProtonDBSession(string host, int port) {
            _client = new TcpClient();
            _client.Connect(host, port);

            var stream = _client.GetStream();
            _reader = new StreamReader(stream, Encoding.UTF8);
            _writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            if (_reader.Peek() >= 0)
                _reader.ReadLine();
        }

        private async Task<ProtonDBResponse> SendAsync(string command, string? data = null) {
            var request = new ProtonDBRequest { Command = command, Data = data };
            string json = JsonSerializer.Serialize(request);
            await _writer.WriteLineAsync(json);

            string? responseLine = await _reader.ReadLineAsync();
            return JsonSerializer.Deserialize<ProtonDBResponse>(responseLine!)!;
        }
        public Task<ProtonDBResponse> QueryAsync(string query) => SendAsync("QUERY", query);
        public Task<ProtonDBResponse> FetchAsync() => SendAsync("FETCH");
        public Task<ProtonDBResponse> DebugAsync(bool enable) => SendAsync("DEBUG", enable.ToString().ToLower());
        public Task<ProtonDBResponse> QuitAsync() => SendAsync("QUIT");
        public Task<ProtonDBResponse> LoginAsync(string? userName, string? password) => SendAsync("LOGIN", $"{userName},{password}");

        public void Dispose() {
            _writer.Dispose();
            _reader.Dispose();
            _client.Close();
        }
    }
}
