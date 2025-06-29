namespace ProtonDB.Server {
    public static class ProtonServer {
        public static async Task Main() {
            var config = ConfigLoader.Load();
            var server = new QueryServer(config.Port);
            await server.StartAsync();
        }
    }
}