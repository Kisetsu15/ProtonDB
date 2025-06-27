using ProtonDB.Server.Core;

namespace ProtonDB.Server {

    public class CommandRegistry {
        private static readonly Dictionary<string, Func<IServerCommand>> commands = new() {
            { Command.login, () => new LoginCommand() },
            { Command.debugOn, () => new DebugCommand(true) },
            { Command.debugOff, () => new DebugCommand(false) },
            { Command.fetch, () => new FetchCommand() },
            { Command.query, () => new QueryCommand() },
            { Command.quit, () => new QuitCommand() }
        };

        public static IServerCommand? Resolve(string input) {
            return commands.TryGetValue(input.Trim().ToUpper(), out var commandFactory)
                ? commandFactory()
                : null;
        }
    }
}