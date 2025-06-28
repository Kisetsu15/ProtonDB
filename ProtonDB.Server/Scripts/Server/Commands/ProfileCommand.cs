using System.Text.Json;

namespace ProtonDB.Server {
    public class ProfileCommand : IServerCommand {
        public async Task ExecuteAsync(QuerySession session, StreamWriter writer, Request request) {
            var profileName = session.CurrentProfile.profileName;
            var profilePrivelege = session.CurrentProfile.profileInfo.Privilege;
            var database = session.CurrentDatabase;

            if (string.IsNullOrWhiteSpace(profileName) || string.IsNullOrWhiteSpace(profilePrivelege) || string.IsNullOrWhiteSpace(database)) {
                await writer.WriteLineAsync(JsonSerializer.Serialize(new Response {
                    Status = "error",
                    Message = "No profile loaded."
                }));
                return;
            }

            var result = new[] {
                profileName,
                profilePrivelege,
                database
            };

            await writer.WriteLineAsync(JsonSerializer.Serialize(new Response {
                Status = "ok",
                Message = "Profile info fetched",
                Result = result
            }));
        }
    }
}
