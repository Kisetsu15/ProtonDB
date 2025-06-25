using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Kisetsu.ProtonDB {

    public class ProtonDBSession : IDisposable {
        private readonly TcpClient _client;
        private readonly StreamReader _reader;
        private readonly StreamWriter _writer;

        public ProtonDBSession(string host = "127.0.0.1", int port = 9090) {
            _client = new TcpClient();
            _client.Connect(host, port);

            var stream = _client.GetStream();
            _reader = new StreamReader(stream, Encoding.UTF8);
            _writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            _reader.ReadLine();
        }

        private async Task<ProtonDBResponse> SendAsync(string command, string? data = null) {
            var request = new ProtonDBRequest { Command = command, Data = data };
            string json = JsonSerializer.Serialize(request);
            await _writer.WriteLineAsync(json);

            string? responseLine = await _reader.ReadLineAsync();
            return JsonSerializer.Deserialize<ProtonDBResponse>(responseLine!)!;
        }
        private Task<ProtonDBResponse> QueryAsync(string query) => SendAsync("QUERY", query);
        private Task<ProtonDBResponse> FetchAsync() => SendAsync("FETCH");
        private Task<ProtonDBResponse> DebugAsync(bool enable) => SendAsync("DEBUG", enable.ToString().ToLower());
        private Task<ProtonDBResponse> QuitAsync() => SendAsync("QUIT");


        public void Query(string query) {
            var response = QueryAsync(query).GetAwaiter().GetResult();
            if (response.Status != "ok") {
                throw new Exception($"Query failed: {response.Message}");
            }
        }

        public string[] FetchAll() {
            var response = FetchAsync().GetAwaiter().GetResult();
            if (response.Result == null || response.Result.Length == 0) {
                return [];
            }
            return response.Result;
        }

        public string FetchOne() {
            var response = FetchAsync().GetAwaiter().GetResult();
            if (response.Result == null || response.Result.Length == 0) {
                return string.Empty;
            }
            return response.Result[0];
        }

        public void Debug(bool enable) {
            var response = DebugAsync(enable).GetAwaiter().GetResult();
            if (response.Status != "ok") {
                throw new Exception($"Debug command failed: {response.Message}");
            }
        }

        public void Quit() {
            var response = QuitAsync().GetAwaiter().GetResult();
            if (response.Status != "ok") {
                throw new Exception($"Quit command failed: {response.Message}");
            }
        }


        public void Dispose() {
            _writer.Dispose();
            _reader.Dispose();
            _client.Close();
        }
    }



}
