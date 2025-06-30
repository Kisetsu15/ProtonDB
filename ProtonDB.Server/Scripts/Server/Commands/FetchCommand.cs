using ProtonDB.Server.Core;
using System.Text.Json;

namespace ProtonDB.Server {
    public class FetchCommand : IServerCommand {
        public async Task ExecuteAsync(QuerySession session, StreamWriter writer, Request request) {
            var res = session.Result ?? ["No stored result to fetch."];
            await writer.WriteLineAsync(JsonSerializer.Serialize(new Response { Result = Meta.Log(res, session) }));
        }
    }

}