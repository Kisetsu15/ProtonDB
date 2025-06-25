namespace ProtonDB.Server {
    public class DebugCommand(bool enable) : IServerCommand {
        private readonly bool _enable = enable;

        public async Task ExecuteAsync(QuerySession session, StreamWriter writer, Request request) {
            session.Debug = _enable;
            await writer.WriteLineAsync($"Debug logs {(_enable ? "enabled" : "disabled")}.");
        }
    }

}