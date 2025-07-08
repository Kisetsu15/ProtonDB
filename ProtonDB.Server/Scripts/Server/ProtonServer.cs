// -------------------------------------------------------------------------------------------------
//  File: ProtonServer.cs
//  Namespace: ProtonDB.Server
//  Description:
//      Contains the main entry point for the ProtonDB server application. This static class
//      initializes core metadata and configuration, creates a QueryServer instance using the
//      configured port, and starts the server asynchronously. The server listens for and
//      processes client connections and commands.
//
//  Public Methods:
//      - Main: The asynchronous entry point for the server. Initializes system state, loads
//        configuration, and starts the main query server loop.
//
//  Usage Example:
//      dotnet run   // Launches the ProtonDB server
//
//  Dependencies:
//      - Meta: Handles initialization of core directories, files, and admin profile.
//      - ConfigLoader: Loads server configuration (e.g., port) from JSON file.
//      - QueryServer: Manages network listening and client command processing.
// -------------------------------------------------------------------------------------------------

using ProtonDB.Server.Core;

namespace ProtonDB.Server {
    /// <summary>
    /// Main entry point for the ProtonDB server application.
    /// Initializes core metadata, loads configuration, and starts the query server.
    /// </summary>
    public static class ProtonServer {
        /// <summary>
        /// Asynchronous entry point. Initializes system state, loads configuration,
        /// creates and starts the QueryServer to handle client connections.
        /// </summary>
        public static async Task Main() {
            Meta.Initialize();
            var config = ConfigLoader.Load();
            var server = new QueryServer(config.Port);
            await server.StartAsync();
        }
    }
}