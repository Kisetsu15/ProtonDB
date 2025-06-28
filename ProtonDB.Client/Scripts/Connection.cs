namespace ProtonDB.Client {
    /// <summary>
    /// Represents a connection to a ProtonDB server, managing session state and authentication.
    /// </summary>
    public class Connection : IDisposable {
        private ProtonSession _session;
        private readonly string _host = defaultHost;
        private readonly int _port = defaultPort;
        private readonly string? _userName = null;
        private readonly string? _password = null;

        /// <summary>
        /// The default host address for ProtonDB connections.
        /// </summary>
        public const string defaultHost = "127.0.0.1";

        /// <summary>
        /// The default port for ProtonDB connections.
        /// </summary>
        public const int defaultPort = 9090;

        /// <summary>
        /// Gets a value indicating whether the connection is currently established.
        /// </summary>
        public bool IsConnected => _session.IsConnected;

        /// <summary>
        /// Gets the underlying ProtonSession for internal use.
        /// </summary>
        internal ProtonSession Session => _session;

        /// <summary>
        /// Initializes a new instance of the <see cref="Connection"/> class.
        /// </summary>
        /// <param name="host">The server host address.</param>
        /// <param name="port">The server port.</param>
        /// <param name="user">The username for authentication (optional).</param>
        /// <param name="pass">The password for authentication (optional).</param>
        private Connection(string host, int port, string? user = null, string? pass = null) {
            _host = host;
            _port = port;
            _userName = user;
            _password = pass;

            _session = new ProtonSession(_host, _port);
            if (_userName != null && _password != null) {
                Login(_userName, _password);
            }
        }

        /// <summary>
        /// Creates and opens a new connection to a ProtonDB server.
        /// </summary>
        /// <param name="host">The server host address (default is 127.0.0.1).</param>
        /// <param name="port">The server port (default is 9090).</param>
        /// <param name="user">The username for authentication (optional).</param>
        /// <param name="pass">The password for authentication (optional).</param>
        /// <returns>A new <see cref="Connection"/> instance.</returns>
        public static Connection Connect(string host = defaultHost, int port = defaultPort, string? user = null, string? pass = null) {
            var connection = new Connection(host, port, user, pass);
            return connection;
        }

        /// <summary>
        /// Authenticates the session with the provided username and password.
        /// </summary>
        /// <param name="userName">The username for authentication.</param>
        /// <param name="password">The password for authentication.</param>
        /// <returns>The server response message.</returns>
        /// <exception cref="Exception">Thrown if login fails.</exception>
        private string Login(string? userName, string? password) {
            var response = _session.LoginAsync(userName, password).GetAwaiter().GetResult();
            if (response.Status != "ok") {
                throw new Exception($"Login failed: {response.Message}");
            }
            return response.Message;
        }

        /// <summary>
        /// Reconnects to the server, re-authenticating if credentials are available.
        /// </summary>
        public void Reconnect() {
            _session.Dispose();
            _session = new ProtonSession(_host, _port);
            if (_userName != null && _password != null) {
                Login(_userName, _password);
            }
        }

        /// <summary>
        /// Disposes the underlying session and releases resources.
        /// </summary>
        public void Dispose() => _session.Dispose();
    }
}