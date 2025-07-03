// -------------------------------------------------------------------------------------------------
//  File: QuitCommand.cs
//  Namespace: ProtonDB.Server
//  Description:
//      Implements the IServerCommand interface to handle the "quit" command, which signals
//      the server to end the current user session. When executed, this command sends a
//      "Goodbye" message to the client, logs the action, and sets the session's ShouldExit
//      property to true, indicating that the server should terminate the connection.
//
//  Public Methods:
//      - ExecuteAsync: Sends a farewell response to the client, logs the event, and marks
//        the session for exit.
//
//  Usage Example (server-side):
//      await new QuitCommand().ExecuteAsync(session, writer, request);
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
    /// Handles the "quit" command to end the current user session and signal server exit.
    /// </summary>
    public class QuitCommand : IServerCommand {
        /// <summary>
        /// Executes the quit command: sends a "Goodbye" message to the client, logs the event,
        /// and sets the session's ShouldExit property to true to terminate the connection.
        /// </summary>
        /// <param name="session">The current user session.</param>
        /// <param name="writer">The stream writer for sending responses to the client.</param>
        /// <param name="request">The incoming request containing the command and data.</param>
        public async Task ExecuteAsync(QuerySession session, StreamWriter writer, Request request) {
            await writer.WriteLineAsync(JsonSerializer.Serialize(new Response {
                Status = "ok",
                Message = Meta.Log("Goodbye")
            }));
            session.ShouldExit = true;
        }
    }
}