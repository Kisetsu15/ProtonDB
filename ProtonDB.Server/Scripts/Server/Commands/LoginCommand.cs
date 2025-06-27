
using Kisetsu.Utils;
using ProtonDB.Server.Core;
using System.Text.Json;

namespace ProtonDB.Server {
    public class LoginCommand : IServerCommand {
        public async Task ExecuteAsync(QuerySession session, StreamWriter writer, Request request) {
            if (string.IsNullOrWhiteSpace(request.Data)) {
                await writer.WriteLineAsync(JsonSerializer.Serialize(new Response {
                    Status = "error",
                    Message = "Username and password required"
                }));

                return;
            }

            string[] args = request.Data.Split(',');
            if (args.Length < 2) {
                await writer.WriteLineAsync(JsonSerializer.Serialize(new Response {
                    Status = "error",
                    Message = "Invalid login format. Use: username,password"
                }));
                return;
            }

            string user = args[0].Trim();
            string pass = args[1].Trim();

            bool success = Profiles.Login(user,pass);

            session.IsAuthenticated = success;
            await writer.WriteLineAsync(JsonSerializer.Serialize(new Response {
                Status = success ? "ok" : "error",
                Message = success ? "Login successful" : "Invalid username or password"
            }));
        }
    }

}