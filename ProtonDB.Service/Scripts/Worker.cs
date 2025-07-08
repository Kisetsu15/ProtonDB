using ProtonDB.Server;
using ProtonDB.Server.Core;

namespace ProtonDB.Service {
    /// <summary>
    /// Worker is a background service that initializes application metadata and starts the QueryServer.
    /// </summary>
    /// <remarks>
    /// This service runs as a hosted background task in a .NET Worker Service environment.
    /// On startup, it initializes the Meta system and launches a QueryServer instance on port 9090.
    /// </remarks>
    public class Worker : BackgroundService {
        /// <summary>
        /// Executes the background service logic.
        /// Initializes Meta and starts the QueryServer asynchronously.
        /// </summary>
        /// <param name="stoppingToken">Token to signal service stop.</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            Meta.Initialize();
            var server = new QueryServer(9090);
            await server.StartAsync();
        }
    }
}