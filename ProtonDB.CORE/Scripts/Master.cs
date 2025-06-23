using Kisetsu.Utils;
using System.Text;

namespace ProtonDB.CORE {

    enum Operation {
        Skip,
        End,
        Noop,
    }

    public static class Master {
        private const int MAX_LENGTH = 4096;
        private const int MAX_ARGUMENT = 1;
        private static readonly Dictionary<string, Func<Operation>> CommandMap = new(StringComparer.OrdinalIgnoreCase) {
            [Token._quit]    = () => { Terminal.WriteLine("Exiting ProtonDB..."); return Operation.End; },
            [Token.quit]     = () => { Terminal.WriteLine("Exiting ProtonDB..."); return Operation.End; },
            [Token.clear]    = () => { Commands.Clear();   return Operation.Skip; },
            [Token._help]    = () => { Commands.Help();    return Operation.Skip; },
            [Token.help]     = () => { Commands.Help();    return Operation.Skip; },
            [Token._version] = () => { Commands.Version(); return Operation.Skip; },
            [Token.version]  = () => { Commands.Version(); return Operation.Skip; }
        };
        public static void Main(string[] args) {
            ProtonMeta.Initialize();

            if (args.Length > MAX_ARGUMENT) {
                Terminal.WriteLine("ProtonDB takes only one argument. Usage: ProtonDB <command>");
                return;
            }

            if (args.Length == MAX_ARGUMENT) {
                var cmdResult = CommandExecutor(args[0]);
                if (cmdResult == Operation.End || cmdResult == Operation.Skip) return;
                if (cmdResult == Operation.Noop) Terminal.WriteLine("Invalid command. Use: '--help'.");
                return;
            }

            while (true) {
                string input = Terminal.Input($"{ProtonMeta.CurrentDatabase}> ");
                var op = CommandExecutor(input);
                if (op == Operation.End) break;
                if (op == Operation.Skip) continue;

                input = MultiLineParser(input);
                Parser.Execute(input);
            }
        }



        private static Operation CommandExecutor(string input) {
            if (string.IsNullOrWhiteSpace(input)) return Operation.Skip;

            input = input.Trim();
            return CommandMap.TryGetValue(input, out var func) ? func() : Operation.Noop;
        }


        private static string MultiLineParser(string input) {
            var sb = new StringBuilder();
            input = input.Trim();
            sb.Append(input);
            
            while (true) {
                if (sb.Length > MAX_LENGTH) {
                    Console.WriteLine("MultiLineParser too long.");
                    return string.Empty;
                }

                if (!string.IsNullOrWhiteSpace(input) && input.EndsWith(')')) break;

                input = Terminal.Input($"{ProtonMeta.CurrentDatabase}= ");
                if (string.IsNullOrWhiteSpace(input)) continue;

                sb.Append(input.Trim());
            }

            return sb.ToString().Trim();
        }

    }
}
