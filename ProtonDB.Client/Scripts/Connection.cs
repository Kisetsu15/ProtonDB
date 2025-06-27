using System.Reflection;

namespace ProtonDB.Client {
    public class Connection : IDisposable {
        private ProtonDBSession _session;
        public const string defaultHost = "127.0.0.1";
        public const int defaultPort = 9090;
        public ProtonDBSession Session => _session;

        private string _host = defaultHost;
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

        
        public void Dispose() {
            _session.Dispose();
        }
    }
}
