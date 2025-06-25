namespace ProtonDB.Server {
    public class ServerConfig {
        public string Host { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 9090;
        public bool Debug { get; set; } = false;
        public int MaxConnections { get; set; } = 100;
    }
}