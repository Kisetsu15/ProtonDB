namespace ProtonDB.Server {

    public class CommandRegistry {
        private static readonly Dictionary<string, Func<IServerCommand>> commands = new() {
            { "DEBUG ON", () => new DebugCommand(true) },
            { "DEBUG OFF", () => new DebugCommand(false) },
            { "FETCH", () => new FetchCommand() },
            { "QUERY", () => new QueryCommand() },
            { "EXIT", () => new ExitCommand() }
        };

        public static IServerCommand? Resolve(string input) {
            return commands.TryGetValue(input.Trim().ToUpper(), out var commandFactory)
                ? commandFactory()
                : null;
        }
    }
}