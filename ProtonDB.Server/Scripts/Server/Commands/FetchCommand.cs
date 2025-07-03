// -------------------------------------------------------------------------------------------------
//  File: FetchCommand.cs
//  Namespace: ProtonDB.Server
//  Description:
//      Implements the IServerCommand interface to handle the "fetch" command, which retrieves
//      the most recent result stored in the user's QuerySession. If no result is available,
//      a default message is returned. The result is logged using Meta.Log and sent to the client
//      as a JSON-serialized Response.
//
//  Public Methods:
//      - ExecuteAsync: Fetches the last stored result from the session, logs it, and sends it
//        to the client in a structured response.
//
//  Usage Example (server-side):
//      await new FetchCommand().ExecuteAsync(session, writer, request);
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
    /// Handles the "fetch" command to retrieve the most recent result from the user's session.
    /// </summary>
    public class FetchCommand : IServerCommand {
        /// <summary>
        /// Fetches the last stored result from the session, logs it, and sends it to the client.
        /// If no result is available, returns a default message.
        /// </summary>
        /// <param name="session">The current user session.</param>
        /// <param name="writer">The stream writer for sending responses to the client.</param>
        /// <param name="request">The incoming request containing the command and data.</param>
        public async Task ExecuteAsync(QuerySession session, StreamWriter writer, Request request) {
            var res = session.Result ?? ["No stored result to fetch."];
            await writer.WriteLineAsync(JsonSerializer.Serialize(new Response { Result = Meta.Log(res, 