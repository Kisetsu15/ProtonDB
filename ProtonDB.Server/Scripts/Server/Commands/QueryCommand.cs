// -------------------------------------------------------------------------------------------------
//  File: QueryCommand.cs
//  Namespace: ProtonDB.Server
//  Description:
//      Implements the IServerCommand interface to handle the "query" command, which processes
//      a user query string, executes it using the Parser, and stores the result in the session.
//      The command logs the query and the acceptance message using Meta.Log. The client is
//      informed that the query was accepted and instructed to use FETCH to retrieve the result.
//
//  Public Methods:
//      - ExecuteAsync: Initializes the session, logs and executes the query, stores the result,
//        and sends a structured response to the client.
//
//  Usage Example (server-side):
//      await new QueryCommand().ExecuteAsync(session, writer, request);
//
//  Dependencies:
//      - IServerCommand: Interface for server command handlers.
//      - QuerySession: Represents the current user session and state.
//      - Request: Encapsulates the incoming client command and data.
//      - Response: Standardized response object for client communication.
//      - Meta: Provides initialization and logging utilities.
//      - Parser: Executes the query and returns the result.
//      - System.Text.Json: Used for serializing responses.
// -------------------------------------------------------------------------------------------------

using ProtonDB.Server.Core;
using System.Text.Json;

namespace ProtonDB.Server {
    /// <summary>
    /// Handles the "query" command to process and execute a user query, storing the result in the session.
    /// </summary>
    public class QueryCommand : IServerCommand {
        /// <summary>
        /// Executes the query command: initializes the session, logs and executes the query,
        /// stores the result, and sends a response indicating the query was accepted.
        /// </summary>
        /// <param name="session">The current user session.</param>
        /// <param name="writer">The stream writer for sending responses to the client.</param>
        /// <param name="request">The incoming request containing the query string.</param>
        public async Task ExecuteAsync(QuerySession session, StreamWriter writer, Request request) {
            Meta.Initialize(session);
            var query = request.Data ?? "";
            session.LastQuery = Meta.Log(query, session);
            //session.Result = Parser.Execute(query, session);
            await writer.WriteLineAsync(JsonSerializer.Serialize(new Response {
                Message = Meta.Log("Query accepted. Use FETCH to retrieve result.")
            }));
        }
    }
}