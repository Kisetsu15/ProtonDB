using Kisetsu.Utils;
using ProtonDB.Server.Core;
using System.Text;
using System.Text.Json;

namespace ProtonDB.Server {

    public interface IServerCommand {
        Task ExecuteAsync(QuerySession session, StreamWriter writer, Request request);
    }


    public class DebugCommand(bool enable) : IServerCommand {
        private readonly bool _enable = enable;

        public async Task ExecuteAsync(QuerySession session, StreamWriter writer, Request request) {
            session.Debug = _enable;
            await writer.WriteLineAsync($"Debug logs {(_enable ? "enabled" : "disabled")}.");
        }
    }

    public class FetchCommand : IServerCommand {
        public async Task ExecuteAsync(QuerySession session, StreamWriter writer, Request request) {
            var res = session.Result ?? ["No stored result to fetch."];
            await writer.WriteLineAsync(JsonSerializer.Serialize(new Response { Result = res }));
        }
    }

    public class QueryCommand : IServerCommand {
        private const int MAX_LENGTH = 128;
        
        public async Task ExecuteAsync(QuerySession session, StreamWriter writer, Request request) {
            ProtonMeta.Initialize();
            var query = request.Data ?? "";
            var parsed = MultiLineParser(query, out bool overflow);
            if (overflow) {
                await writer.WriteLineAsync(JsonSerializer.Serialize(new Response {
                    Status = "error",
                    Message = "Input too long"
                }));
            }
            session.LastQuery = parsed;
            session.Result = Parser.Execute(parsed);
            await writer.WriteLineAsync(JsonSerializer.Serialize(new Response {
                Message = "Query accepted. Use FETCH to retrieve result."
            }));
        }


        private static string MultiLineParser(string input, out bool overflow) {
            overflow = false;
            var sb = new StringBuilder();
            input = input.Trim();
            sb.Append(input);

            while (true) {
                if (sb.Length > MAX_LENGTH) {
                    overflow = true;
                    return ("Input is too long");
                }

                if (!string.IsNullOrWhiteSpace(input) && input.EndsWith(')')) break;
                input = Terminal.Input($"{ProtonMeta.CurrentDatabase}= ");
                if (string.IsNullOrWhiteSpace(input)) continue;

                sb.Append(input.Trim());
            }
            return sb.ToString().Trim();
        }
    }

    public class ExitCommand : IServerCommand {
        public async Task ExecuteAsync(QuerySession session, StreamWriter writer, Request request) {
            await writer.WriteLineAsync("Goodbye.");
            session.ShouldExit = true;
        }
    }

}