using System.Text.Json;

namespace ProtonDB.Server {

    public class QuitCommand : IServerCommand {
        public async Task ExecuteAsync(QuerySession session, StreamWriter writer, Request request) {
            await writer.WriteLineAsync(JsonSerializer.Serialize(new Response { 
                Status = "ok",
                Message = "Goodbye." 
            }));
            session.ShouldExit = true;
        }
    }

}