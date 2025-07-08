// -------------------------------------------------------------------------------------------------
//  File: CommandRegistry.cs
//  Namespace: ProtonDB.Server
//  Description:
//      Provides a centralized registry for mapping command keywords to their corresponding
//      IServerCommand implementations. The registry enables dynamic resolution and instantiation
//      of server command handlers based on incoming client requests. This design supports
//      extensibility and decouples command parsing from command execution logic.
//
//  Public Methods:
//      - Resolve: Given a command keyword (case-insensitive), returns an instance of the
//        corresponding IServerCommand, or null if the command is not registered.
//
//  Usage Example:
//      var command = CommandRegistry.Resolve(request.Command);
//      if (command != null) await command.ExecuteAsync(session, writer, request);
//
//  Dependencies:
//      - IServerCommand: Interface for server command handlers.
//      - Command: Static class containing command keyword constants (e.g., "LOGIN", "QUERY", etc.).
//      - Individual command classes: LoginCommand, DebugCommand, FetchCommand, QueryCommand,
//        ProfileCommand, QuitCommand.
// -------------------------------------------------------------------------------------------------

using ProtonDB.Server.Core;

namespace ProtonDB.Server {

    /// <summary>
    /// Centralized registry for mapping command keywords to their IServerCommand implementations.
    /// Enables dynamic resolution and instantiation of server command handlers.
    /// </summary>
    public class CommandRegistry {
        private static readonly Dictionary<string, Func<IServerCommand>> commands = new() {
            { Command.login, () => new LoginCommand() },
            { Command.debug, () => new DebugCommand() },
            { Command.fetch, () => new FetchCommand() },
            { Command.query, () => new QueryCommand() },
            { Command.profile, () => new ProfileCommand() },
            { Command.quit, () => new QuitCommand() }
        };

        /// <summary>
        /// Resolves a command keyword to its corresponding IServerCommand instance.
        /// Returns null if the command is not registered.
        /// </summary>
        /// <param name="input">The command keyword (case-insensitive).</param>
        /// <returns>An instance of the corresponding IServerCommand, or null if not found.</returns>
        public static IServerCommand? Resolve(string input) {
            return commands.TryGetValue(input.Trim().ToUpper(), out var commandFactory)
                ? commandFactory()
                : null;
        }
    }
}