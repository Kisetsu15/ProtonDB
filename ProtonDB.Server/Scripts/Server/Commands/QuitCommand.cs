namespace ProtonDB.Server {

    public class QuitCommand : IServerCommand {
        public async Task ExecuteAsync(QuerySession session, StreamWriter writer, Request request) {
            await writer.WriteLineAsync("Goodbye.");
            session.ShouldExit = true;
        }
    }

}