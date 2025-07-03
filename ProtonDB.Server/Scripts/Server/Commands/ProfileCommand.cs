// -------------------------------------------------------------------------------------------------
//  File: ProfileCommand.cs
//  Namespace: ProtonDB.Server
//  Description:
//      Implements the IServerCommand interface to handle the "profile" command, which retrieves
//      and returns the current user's profile information for the active session. The command
//      fetches the profile name, privilege, and current database, and sends them to the client
//      as a JSON-serialized Response. If any required profile information is missing, an error
//      response is returned.
//
//  Public Methods:
//      - ExecuteAsync: Gathers the current profile's name, privilege, and database from the session,
//        validates their presence, and sends a structured response to the client.
//
//  Usage Example (server-side):
//      await new ProfileCommand().ExecuteAsync(session, writer, request);
//
//  Dependencies:
//      - IServerCommand: Interface for server command handlers.
//      - QuerySession: Represents the current user session and state.
//      - Profile: Contains the user's profile name and profile info (including privilege).
//      - Request: Encapsulates the incoming client command and data.
//      - Response: Standardized response object for client communication.
//      - System.Text.Json: Used for serializing responses.
// -------------------------------------------------------------------------------------------------

using System.Text.Json;

namespace ProtonDB.Server {
    /// <summary>
    /// Handles the "profile" command to fetch and return the current user's profile information.
    /// </summary>
    public class ProfileCommand : IServerCommand {
        /// <summary>
        /// Executes the profile command, returning the current profile's name, privilege, and database.
        /// Sends a JSON response with the profile info or an error if not available.
        /// </summary>
        /// <param name="session">The current user session.</param>
        /// <param name="writer">The stream writer for sending responses to the client.</param>
        /// <param name="request">The incoming request containing the command and data.</param>
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