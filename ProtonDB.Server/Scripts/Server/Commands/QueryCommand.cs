using ProtonDB.Server.Core;
using System.Text.Json;

namespace ProtonDB.Server {
    public class QueryCommand : IServerCommand {
        public async Task ExecuteAsync(QuerySession session, StreamWriter writer, Request request) {
            Meta.Initialize();
            var query = request.Data ?? "";
            session.LastQuery = query;
            session.Result = Parser.Execute(query);
            await writer.WriteLineAsync(JsonSerializer.Serialize(new Response {
                Message = "Query accepted. Use FETCH to retrieve result."
            }));
        }
    }

}