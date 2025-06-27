
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
                Terminal.Log("Login command received with no data.");
                return;
            }

            string[] args = request.Data.Split(',');
            if (args.Length < 2) {
                await writer.WriteLineAsync(JsonSerializer.Serialize(new Response {
                    Status = "error",
                    Message = "Invalid login format. Use: username,password"
                }));
                Terminal.Log("Login command received with invalid format.");
                return;
            }

            string user = args[0].Trim();
            string pass = args[1].Trim();

            bool success = Profiles.Login(user,pass);

            session.IsAuthenticated = success;
            Terminal.Log($"Login attempt for user '{user}': '{pass}' : {(success ? "Success" : "Failure")}");
            await writer.WriteLineAsync(JsonSerializer.Serialize(new Response {
                Status = success ? "ok" : "error",
                Message = success ? "Login successful" : "Invalid username or password"
            }));
        }
    }

}