// -------------------------------------------------------------------------------------------------
//  File: LoginCommand.cs
//  Namespace: ProtonDB.Server
//  Description:
//      Implements the IServerCommand interface to handle the "login" command, which authenticates
//      a user based on provided credentials. The command expects a comma-separated username and
//      password in the request data. If authentication succeeds, the session is updated and a
//      success response is sent; otherwise, an error response is returned. All login attempts
//      are logged using Meta.Log.
//
//  Public Methods:
//      - ExecuteAsync: Processes the login request, validates input, attempts authentication via
//        Profiles.Login, updates session state, logs the result, and sends a structured response
//        to the client.
//
//  Usage Example (server-side):
//      await new LoginCommand().ExecuteAsync(session, writer, request);
//
//  Dependencies:
//      - IServerCommand: Interface for server command handlers.
//      - QuerySession: Represents the current user session and state.
//      - Request: Encapsulates the incoming client command and data.
//      - Response: Standardized response object for client communication.
//      - Profiles: Provides authentication logic.
//      - Meta: Provides logging utilities.
//      - System.Text.Json: Used for serializing responses.
// -------------------------------------------------------------------------------------------------

using ProtonDB.Server.Core;
using System.Text.Json;

namespace ProtonDB.Server {
    /// <summary>
    /// Handles the "login" command to authenticate a user and update the session state.
    /// </summary>
    public class LoginCommand : IServerCommand {
        /// <summary>
        /// Executes the login command, authenticating the user with the provided credentials.
        /// Expects a comma-separated username and password in the request data.
        /// Sends a JSON response indicating success or error, and logs the attempt.
        /// </summary>
        /// <param name="session">The current user session.</param>
        /// <param name="writer">The stream writer for sending responses to the client.</param>
        /// <param name="request">The incoming request containing the command and data.</param>
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

            bool success = Profiles.Login(user, pass, session);

            session.IsAuthenticated = success;
            await writer.WriteLineAsync(JsonSerializer.Serialize(new Response {
                Status = success ? "ok" : "error",
                Message = Meta.Log(success ? "Login successful" : "Invalid username or password", session)
            }));
        }
    }
}