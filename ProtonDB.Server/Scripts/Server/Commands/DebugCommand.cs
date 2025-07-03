// -------------------------------------------------------------------------------------------------
//  File: DebugCommand.cs
//  Namespace: ProtonDB.Server
//  Description:
//      Implements the IServerCommand interface to handle the "debug" command, which toggles
//      debug logging for the current QuerySession. The command expects a boolean value ("true"
//      or "false") in the request data. The result is sent to the client as a JSON-serialized
//      Response, and the action is logged using Meta.Log.
//
//  Public Methods:
//      - ExecuteAsync: Processes the debug toggle request, validates input, updates the session's
//        Debug property, logs the action, and sends a structured response to the client.
//
//  Usage Example (server-side):
//      await new DebugCommand().ExecuteAsync(session, writer, request);
//
//  Dependencies:
//      - IServerCommand: Interface for server command handlers.
//      - QuerySession: Represents the current user session and state.
//      - Request: Encapsulates the incoming client command and data.
//      - Response: Standardized response object for client communication.
//      - Meta: Provides logging utilities.
//      - System.Text.Json: Used for serializing responses.
// -------------------------------------------------------------------------------------------------

using ProtonDB.Server.Core;
using System.Text.Json;

namespace ProtonDB.Server {
    /// <summary>
    /// Handles the "debug" command to enable or disable debug logging for the current session.
    /// </summary>
    public class DebugCommand : IServerCommand {

        /// <summary>
        /// Executes the debug command, toggling debug logging for the session.
        /// Expects a boolean argument ("true" or "false") in the request data.
        /// Sends a JSON response indicating success or error.
        /// </summary>
        /// <param name="session">The current user session.</param>
        /// <param name="writer">The stream writer for sending responses to the client.</param>
        /// <param name="request">The incoming request containing the command and data.</param>
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
                Message = Meta.Log($"Debug logs {(_enable ? "enabled" : "disabled")}", session)
            }));
        }
    }
}