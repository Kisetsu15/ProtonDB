// -------------------------------------------------------------------------------------------------
//  File: Request.cs
//  Namespace: ProtonDB.Server
//  Description:
//      Defines the Request class, which represents a client request sent to the ProtonDB server.
//      Each request contains a command keyword and optional data payload. This class is used
//      throughout the server to deserialize incoming JSON messages from clients and route them
//      to the appropriate command handler.
//
//  Public Properties:
//      - Command: The command keyword specifying the action to perform (e.g., "LOGIN", "QUERY").
//      - Data: Optional data or arguments associated with the command.
//
//  Usage Example (client sends JSON):
//      { "Command": "LOGIN", "Data": "username,password" }
//
//  Dependencies:
//      None (POCO class for request transport).
// -------------------------------------------------------------------------------------------------

namespace ProtonDB.Server {
    /// <summary>
    /// Represents a client request sent to the ProtonDB server.
    /// Contains the command keyword and optional data payload.
    /// </summary>
    public class Request {
        /// <summary>
        /// The command keyword specifying the action to perform (e.g., "LOGIN", "QUERY").
        /// </summary>
        public string? Command { get; set; }

        /// <summary>
        /// Optional data or arguments associated with the command.
        /// </summary>
        public string? Data { get; set; }
    }
}