using System.Text.Json;

namespace ProtonDB.Server {
    public class DebugCommand : IServerCommand {

        public async Task ExecuteAsync(QuerySession session, StreamWriter writer, Request request) {
            if (string.IsNullOrWhiteSpace(request.Data)) {
                await writer.WriteLineAsync(JsonSerializer.Serialize(new Response {
                    Status = "error",
                    Message = "Toggle required"
                }));
                return;
            }

            if (!bool.TryParse(request.Data, out bool _enable)) {
                await writer.WriteLineAsync(JsonSerializer.Serialize(new Response {
                    Status = "error",
                    Message = "Invalid argument. Use true or false."
                }));
                return;
            }

            session.Debug = _enable;
            await writer.WriteLineAsync(JsonSerializer.Serialize(new Response {
                Status = "ok",
                Message = $"Debug logs {(_enable ? "enabled" : "disabled")}"
            }));
        }
    }

}