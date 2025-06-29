using Kisetsu.Utils;
using System.Text;
using ProtonDB.Client;

namespace ProtonDB.Shell {


    public static class Master {

        private const int MAX_LENGTH = 4096;
        private const int MAX_ARGUMENT = 1;

        private enum Operation {
            Skip,
            End,
            Noop,
        }

        private static readonly Dictionary<string, Func<Operation>> CommandMap = new(StringComparer.OrdinalIgnoreCase) {
            [Token._quit]    = () => { Commands.Quit(); return Operation.End; },
            [Token.quit]     = () => { Commands.Quit(); return Operation.End; },
            [Token.clear]    = () => { Commands.Clear();   return Operation.Skip; },
            [Token._help]    = () => { Commands.Help();    return Operation.Skip; },
            [Token.help]     = () => { Commands.Help();    return Operation.Skip; },
            [Token._version] = () => { Commands.Version(); return Operation.Skip; },
            [Token.version]  = () => { Commands.Version(); return Operation.Skip; },
            ["test"] = () => {
                Terminal.WriteLine("Testing mode is not implemented yet.");
                return Operation.Skip;
            },
            ["reload"] = () => {
                ProtonMeta.Loading();
                return Operation.Skip;
            }
        };

        public static void Main(string[] args) {
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

            
            Connection? connection = CreateConnection();
            if (connection == null) {
                Terminal.WriteLine("Failed to connect to ProtonDB server. Please check your connection settings.", ConsoleColor.Red);
                return;
            }
            Cursor cursor = new(connection);
            Console.WriteLine();

            while (true) {
                if(!PrintProfileDetails(cursor.Profile())) break;

                string input = Terminal.Input(ConsoleColor.White, "$ ");
                if (string.IsNullOrWhiteSpace(input)) { 
                    Console.WriteLine(); 
                    continue; 
                }
                var op = CommandExecutor(input);
                if (op == Operation.End) break;
                if (op == Operation.Skip) continue;

                input = MultiLineParser(input);
                cursor.SafeQuery(input);
                string[] output = cursor.FetchAll();
                if (output.Length == 0) {
                    Terminal.WriteLine("No results found.", ConsoleColor.Yellow);
                } else {
                    foreach (var line in output) {
                        Terminal.WriteLine(line);
                    }
                }

                Console.WriteLine();
            }
            cursor.Quit();
        }

        private static Connection? CreateConnection() {
            string _host = Terminal.Input($"Host [{Connection.defaultHost}]: ");
            if (string.IsNullOrWhiteSpace(_host)) _host = Connection.defaultHost;
            string port = (Terminal.Input($"Port [{Connection.defaultPort}]: "));
            int _port = string.IsNullOrWhiteSpace(port) ? Connection.defaultPort : int.TryParse(port, out int parsedPort) ? parsedPort : -1;
            if (_port <= 0) _port = Connection.defaultPort;

            string _username = "dharshik"; //Terminal.Input($"Username: ");
            string _password = "welcome"; //Terminal.Input($"Password: ");

            if (string.IsNullOrWhiteSpace(_username) || string.IsNullOrWhiteSpace(_password)) {
                Terminal.WriteLine("Username and password cannot be empty.", ConsoleColor.Red);
                return null;
            }

            return Connection.Connect(_host, _port, _username, _password);
        }

        private static bool PrintProfileDetails(string[] details) {
            if (details == null || details.Length == 0) {
                Terminal.WriteLine("Profile not found", ConsoleColor.Red);
                return false;
            }

            int usernameIndex = 0;
            int privelegeIndex = 1;
            int currentDatabaseIndex = 2;

            Terminal.Write($"{details[privelegeIndex]}@{details[usernameIndex]}", ConsoleColor.Green);
            Terminal.Write($" » ", ConsoleColor.Yellow);
            Terminal.WriteLine($"({details[currentDatabaseIndex]})", ConsoleColor.Cyan);
            Console.ResetColor();
            return true;
        }


        private static Operation CommandExecutor(string input) {
            input = input.Trim();
            return CommandMap.TryGetValue(input, out var func) ? func() : Operation.Noop;
        }


        private static string MultiLineParser(string input) {
            var sb = new StringBuilder();
            bool isValid = true;
            input = input.Trim();
            sb.Append(input);
            while (true) {
                if (sb.Length > MAX_LENGTH) {
                    Console.WriteLine("MultiLine Query too long.");
                    return string.Empty;
                }

                if (!string.IsNullOrWhiteSpace(input) && input.EndsWith(')')) break;

                input = Terminal.Input(ConsoleColor.White, $"- ").Trim();
                if (string.IsNullOrWhiteSpace(input)) continue;
                if (char.IsLetter(sb[^1]) && char.IsLetter(input[0])) {
                    isValid = false;
                }
                sb.Append(input.Trim());
            }
            return isValid ? sb.ToString().Trim() : string.Empty;
        }

    }
}
