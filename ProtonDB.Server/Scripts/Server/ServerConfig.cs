namespace ProtonDB.Server {
    /// <summary>
    /// Represents the configuration settings for the ProtonDB server.
    /// </summary>
    public class ServerConfig {
        /// <summary>
        /// Gets or sets the hostname or IP address the server will bind to.
        /// Default is "127.0.0.1".
        /// </summary>
        public string Host { get; set; } = "127.0.0.1";

        /// <summary>
        /// Gets or sets the port number the server will listen on.
        /// Default is 9090.
        /// </summary>
        public int Port { get; set; } = 9090;

        /// <summary>
        /// Gets or sets a value indicating whether debug mode is enabled.
        /// Default is false.
        /// </summary>
        public bool Debug { get; set; } = false;

        /// <summary>
        /// Gets or sets the maximum number of simultaneous connections allowed.
        /// Default is 100.
        /// </summary>
        public int MaxConnections { get; set; } = 100;
    }
}