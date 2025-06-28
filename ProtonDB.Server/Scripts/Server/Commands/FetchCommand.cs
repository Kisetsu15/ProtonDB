using System.Text.Json;

namespace ProtonDB.Server {
    public class FetchCommand : IServerCommand {
        public async Task ExecuteAsync(QuerySession session, StreamWriter writer, Request request) {
            var res = session.Result ?? ["No stored result to fetch."];
            Console.WriteLine($"Fetching result: {string.Join(", ", res)}");
            await writer.WriteLineAsync(JsonSerializer.Serialize(new Response { Result = res }));
        }
    }

}