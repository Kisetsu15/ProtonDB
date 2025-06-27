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

    public class Connection : IDisposable {
        private ProtonDBSession _session;
        public const string defaultHost = "127.0.0.1";
        public const int defaultPort = 9090;

        private readonly string _host = defaultHost;
        private readonly int _port = defaultPort;
        private readonly string? _userName = null;
        private readonly string? _password = null;

        private Connection (string host, int port, string? user = null, string? pass = null) {
            _host = host;
            _port = port;
            _userName = user;
            _password = pass;

            _session = new ProtonDBSession(_host, _port);
            if (_userName != null && _password != null) {
                Login(_userName, _password);
            }
        }

        public static Connection Connect(string host = defaultHost, int port = defaultPort, string? user = null, string? pass = null) {
            var connection = new Connection(host,port,user,pass);
            return connection;
        }


        private string Login(string? userName, string? password) {
            var response = _session.LoginAsync(userName, password).GetAwaiter().GetResult();
            if (response.Status != "ok") {
                throw new Exception($"Login failed: {response.Message}");
            }
            return response.Message;
        }

        public bool IsConnected => _session.IsConnected;

        public void Reconnect() {
            _session.Dispose();
            _session = new ProtonDBSession(_host, _port);
            if (_userName != null && _password != null) {
                Login(_userName, _password);
            }
        }


        public void Query(string query) {
            var response = _session.QueryAsync(query).GetAwaiter().GetResult();
            if (response.Status != "ok") {
                throw new Exception($"Query failed: {response.Message}");
            }
        }

        public void SafeQuery(string query) {
            try {
                Query(query);
            } catch (Exception) {
                Reconnect();
                Query(query);
            }
        }

        public string QueryRaw(string query) {
            var res = _session.QueryAsync(query).GetAwaiter().GetResult();
            return res.Message;
        }


        public string[] FetchAll() {
            var response = _session.FetchAsync().GetAwaiter().GetResult();
            if (response.Result == null || response.Result.Length == 0) {
                return [];
            }
            return response.Result;
        }

        public string FetchOne() {
            var response = _session.FetchAsync().GetAwaiter().GetResult();
            if (response.Result == null || response.Result.Length == 0) {
                return string.Empty;
            }
            return response.Result[0];
        }

        public void Debug(bool enable) {
            var response = _session.DebugAsync(enable).GetAwaiter().GetResult();
            if (response.Status != "ok") {
                throw new Exception($"Debug command failed: {response.Message}");
            }
        }

        public void Quit() {
            var response = _session.QuitAsync().GetAwaiter().GetResult();
            if (response.Status != "ok") {
                throw new Exception($"Quit command failed: {response.Message}");
            }
        }

        public void Dispose() {
            _session.Dispose();
        }
    }



}
