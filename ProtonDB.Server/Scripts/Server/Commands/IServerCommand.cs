// -------------------------------------------------------------------------------------------------
//  File: IServerCommand.cs
//  Namespace: ProtonDB.Server
//  Description:
//      Defines the interface for server command handlers in ProtonDB. Implementations of this
//      interface encapsulate the logic for processing specific client commands, such as query,
//      login, fetch, debug, and profile management. Each command operates asynchronously and
//      interacts with the current user session, the network stream, and the incoming request.
//
//  Interface Methods:
//      - ExecuteAsync: Asynchronously executes the command logic, using the provided session,
//        writer, and request. Implementations are responsible for validating input, updating
//        session state, and sending structured responses to the client.
//
//  Parameters:
//      - QuerySession session: The current user session, containing authentication state,
//        user profile, and session-specific data.
//      - StreamWriter writer: The network stream writer for sending responses to the client.
//      - Request request: The incoming client request, including the command and any arguments.
//
//  Usage Example:
//      public class MyCommand : IServerCommand {
//          public async Task ExecuteAsync(QuerySession session, StreamWriter writer, Request request) {
//              // Command logic here
//          }
//      }
//
//  Dependencies:
//      - QuerySession: Represents the current user session and state.
//      - Request: Encapsulates the incoming client command and data.
//      - StreamWriter: Used for network communication with the client.
// -------------------------------------------------------------------------------------------------

namespace ProtonDB.Server {
    /// <summary>
    /// Defines the interface for server command handlers in ProtonDB.
    /// Implementations process specific client commands asynchronously,
    /// interacting with the user session, network stream, and request data.
    /// </summary>
    public interface IServerCommand {
        /// <summary>
        /// Asynchronously executes the command logic for the given session and request,
        /// writing the response to the provided stream writer.
        /// </summary>
        /// <param name="session">The current user session.</param>
        /// <param name="writer">The stream writer for sending responses to the client.</param>
        /// <param name="request">The incoming client request.</param>
        Task ExecuteAsync(QuerySession session, StreamWriter writer, Request request);
    }
}