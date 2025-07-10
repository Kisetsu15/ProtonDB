// -------------------------------------------------------------------------------------------------
//  File: Master.cs
//  Namespace: ProtonDB.Shell
//  Description:
//      Provides the main entry point and command loop for the ProtonDB shell. Handles command-line
//      arguments, user authentication, command execution, and interactive querying. This class
//      manages the interactive session lifecycle, including connection setup, command parsing,
//      multi-line query support, and output formatting.
//
//  Public Methods:
//      - Main: Entry point for the ProtonDB shell. Handles argument parsing, connection setup, and
//        the main interactive loop.
//
//  Private Methods:
//      - PrintOutput: Prints the output of the last query executed by the cursor.
//      - CreateConnection: Prompts the user for connection details and attempts to establish a
//        connection to the ProtonDB server.
//      - PrintProfileDetails: Prints the current user's profile details in the shell prompt.
//      - CommandExecutor: Executes a command if the input matches a known command token.
//      - MultiLineParser: Handles multi-line input for queries, allowing the user to continue input
//        until a valid query is formed.
//
//  Usage Example:
//      Master.Main(args);
//
//  Dependencies:
//      - Kisetsu.Utils.Terminal: For terminal output and input handling.
//      - ProtonDB.Client.Connection, Cursor: For database connection and query execution.
//      - Token, Commands: For command token definitions and built-in shell commands.
// -------------------------------------------------------------------------------------------------

using Kisetsu.Utils;
using System.Text;
using ProtonDB.Client;

namespace ProtonDB.Shell {

    /// <summary>
    /// Provides the main entry point and command loop for the ProtonDB shell.
    /// Handles command-line arguments, user authentication, command execution, and interactive querying.
    /// </summary>
    public static class Master {

        private const int MAX_LENGTH = 4096;
        private const int MAX_ARGUMENT = 1;

        /// <summary>
        /// Represents the result of a command operation.
        /// </summary>
        private enum Operation {
            /// <summary>Skip further processing for this input.</summary>
            Skip,
            /// <summary>End the shell session.</summary>
            End,
            /// <summary>No operation matched; input was not a recognized command.</summary>
            Noop,
        }

        /// <summary>
        /// Maps command tokens to their corresponding actions and operation results.
        /// </summary>
        private static readonly Dictionary<string, Func<Operation>> CommandMap = new(StringComparer.OrdinalIgnoreCase) {
            [Token._quit] = () => { Commands.Quit(); return Operation.End; },
            [Token.quit] = () => { Commands.Quit(); return Operation.End; },
            [Token.clear] = () => { Commands.Clear(); return Operation.Skip; },
            [Token._help] = () => { Commands.Help(); return Operation.Skip; },
            [Token.help] = () => { Commands.Help(); return Operation.Skip; },
            [Token._version] = () => { Commands.Version(); return Operation.Skip; },
            [Token.version] = () => { Commands.Version(); return Operation.Skip; },
        };

        /// <summary>
        /// Entry point for the ProtonDB shell.
        /// Handles argument parsing, connection setup, and the main interactive loop.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        public static void Main(string[] args) {
            Meta.Loading();
            if (args.Length > MAX_ARGUMENT) {
                Terminal.WriteLine("ProtonDB takes only one argument. Usage: ProtonDB <command>");
                return;
            }

            if (args.Length == MAX_ARGUMENT) {
                string input = args[0].Trim().ToLower();
                if (input == Token.quit || input == Token._quit) return;
                var cmdResult = CommandExecutor(input);
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
                if (!PrintProfileDetails(cursor.Profile())) break;

                string input = Terminal.Input(ConsoleColor.White, "$ ");
                if (string.IsNullOrWhiteSpace(input)) {
                    Console.WriteLine();
                    continue;
                }
                var op = CommandExecutor(input);
                if (op == Operation.End) break;
                if (op == Operation.Skip) continue;

                input = MultiLineParser(input);
                cursor.Query(input);
                cursor.PrintOutput();
            }
            cursor.Quit();
        }

        /// <summary>
        /// Prints the output of the last query executed by the cursor.
        /// </summary>
        /// <param name="cursor">The cursor instance.</param>
        private static void PrintOutput(this Cursor cursor) {
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

        /// <summary>
        /// Prompts the user for connection details and attempts to establish a connection to the ProtonDB server.
        /// </summary>
        /// <returns>A <see cref="Connection"/> object if successful; otherwise, <c>null</c>.</returns>
        private static Connection? CreateConnection() {
            string _host = Terminal.Input($"Host [{Connection.defaultHost}]: ");
            if (string.IsNullOrWhiteSpace(_host)) _host = Connection.defaultHost;
            string port = (Terminal.Input($"Port [{Connection.defaultPort}]: "));
            int _port = string.IsNullOrWhiteSpace(port) ? Connection.defaultPort : int.TryParse(port, out int parsedPort) ? parsedPort : -1;
            if (_port <= 0) _port = Connection.defaultPort;

            string _username = Terminal.Input($"Username: ");
            string _password = Terminal.Input($"Password: ");

            if (string.IsNullOrWhiteSpace(_username) || string.IsNullOrWhiteSpace(_password)) {
                Terminal.WriteLine("Username and password cannot be empty.", ConsoleColor.Red);
                return null;
            }
            return Connection.Connect(_host, _port, _username, _password);
        }

        /// <summary>
        /// Prints the current user's profile details in the shell prompt.
        /// </summary>
        /// <param name="details">An array containing profile details: privilege, username, and current database.</param>
        /// <returns><c>true</c> if details are valid and printed; otherwise, <c>false</c>.</returns>
        private static bool PrintProfileDetails(string[] details) {
            if (details == null || details.Length == 0) {
                Terminal.WriteLine("Profile not found", ConsoleColor.Red);
                return false;
            }

            int usernameIndex = 0;
            int privelegeIndex = 1;
            int currentDatabaseIndex = 2;

            Terminal.Write($"{details[privelegeIndex]}_{details[usernameIndex]}", ConsoleColor.Green);
            Terminal.Write($" » ", ConsoleColor.Yellow);
            Terminal.WriteLine($"({details[currentDatabaseIndex]})", ConsoleColor.Cyan);
            Console.ResetColor();
            return true;
        }

        /// <summary>
        /// Executes a command if the input matches a known command token.
        /// </summary>
        /// <param name="input">The user input string.</param>
        /// <returns>The <see cref="Operation"/> result of the command execution.</returns>
        private static Operation CommandExecutor(string input) {
            input = input.Trim();
            return CommandMap.TryGetValue(input, out var func) ? func() : Operation.Noop;
        }

        /// <summary>
        /// Handles multi-line input for queries, allowing the user to continue input until a valid query is formed.
        /// </summary>
        /// <param name="input">The initial input string.</param>
        /// <returns>The complete query string if valid; otherwise, an empty string.</returns>
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

                input = Terminal.Input(ConsoleColor.White, $"> ").Trim();
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