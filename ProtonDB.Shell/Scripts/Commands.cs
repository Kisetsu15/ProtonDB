// -------------------------------------------------------------------------------------------------
//  File: Commands.cs
//  Namespace: ProtonDB.Shell
//  Description:
//      Provides built-in shell commands for ProtonDB, including displaying help, version information,
//      clearing the console, and exiting the application. This static class centralizes user-facing
//      command logic for the interactive shell, ensuring consistent behavior and output formatting.
//
//  Public Methods:
//      - Version: Displays the current version of ProtonDB in the terminal.
//      - Help:    Prints usage instructions and a list of available commands with descriptions.
//      - Quit:    Prints an exit message to the terminal, indicating that ProtonDB is closing.
//      - Clear:   Clears the console window.
//
//  Usage Example:
//      Commands.Help();      // Show help information
//      Commands.Version();   // Show version information
//      Commands.Quit();      // Print exit message
//      Commands.Clear();     // Clear the console
//
//  Dependencies:
//      - Kisetsu.Utils.Terminal: For terminal output and input handling.
//      - System.Reflection: For retrieving assembly version information.
// -------------------------------------------------------------------------------------------------

using Kisetsu.Utils;
using System.Reflection;

namespace ProtonDB.Shell {
    /// <summary>
    /// Provides built-in shell commands for ProtonDB, such as displaying help, version information, clearing the console, and exiting the application.
    /// </summary>
    public static class Commands {
        /// <summary>
        /// Displays the current version of ProtonDB in the terminal.
        /// </summary>
        public static void Version() => Terminal.WriteLine($" ProtonDB v{Assembly.GetExecutingAssembly().GetName().Version}\n");

        /// <summary>
        /// Displays help information, including usage, available commands, and their descriptions.
        /// </summary>
        public static void Help() {
            Terminal.WriteLine("\nUsage: protondb <command>");
            Terminal.WriteLine("\nCommands:");
            Terminal.WriteLine("  protondb                          Open ProtonDB");
            Terminal.WriteLine("  :h, --help                        Show this help message");
            Terminal.WriteLine("  :v, --version                     Show ProtonDB version");
            Terminal.WriteLine("  :q, quit                          Exit ProtonDB");
            Terminal.WriteLine("  cls                               Clear Console");

            Terminal.WriteLine("\nProfile Operations: profile.<operation>(argument)");
            Terminal.WriteLine("  create(name,password,privilege)   Create a new profile");
            Terminal.WriteLine("  delete(name)                      Drop a profiles");
            Terminal.WriteLine("  grant(db_name)                    Grant access to database");
            Terminal.WriteLine("  revoke(db_name)                   revoke access to database");
            Terminal.WriteLine("  list()                            List all profiles");

            Terminal.WriteLine("\nDatabase Operations: db.<operation>(argument)");
            Terminal.WriteLine("  use(name)                         Use a database");
            Terminal.WriteLine("  create(name)                      Create a new database");
            Terminal.WriteLine("  drop()                            Drop the current database");
            Terminal.WriteLine("  drop(name)                        Drop a specified database");
            Terminal.WriteLine("  list()                            List all databases");

            Terminal.WriteLine("\nCollection Operations: collection.<operation>(argument)");
            Terminal.WriteLine("  create(<collection>)                          Create a new collection");
            Terminal.WriteLine("  drop(<collection>)                            Drop a collection");
            Terminal.WriteLine("  list()                                        List all collections in the current database");
            Terminal.WriteLine("  list(<database>)                              List all collections in the specified database");

            Terminal.WriteLine("\nDocument Operations: <collection_name>.<operation>(argument)");
            Terminal.WriteLine("  insert(data)                      Insert a document into collection");
            Terminal.WriteLine("  remove()                          Remove all documents in a collection");
            Terminal.WriteLine("  remove(condition)                 Remove documents matching the condition from collection");
            Terminal.WriteLine("  print()                           Print all documents in collection");
            Terminal.WriteLine("  print(condition)                  Print documents matching the condition from collection");
            Terminal.WriteLine("  update(action, data)              Update all documents in collection");
            Terminal.WriteLine("  update(action, data, condition)   Update documents matching the condition in collection");

            Terminal.WriteLine("\n  action    - [ add | drop | alter ]");
            Terminal.WriteLine("  data      -  {\"key\": value}");
            Terminal.WriteLine("  condition - key <operator> value");
            Terminal.WriteLine("  operators - [ < | <= | > | >= | = ]");
            Terminal.WriteLine("*Note*: data is {\"key\"} for update(drop, data, condition)\n");

        }

        /// <summary>
        /// Prints an exit message to the terminal, indicating that ProtonDB is closing.
        /// </summary>
        public static void Quit() => Terminal.WriteLine("Exiting ProtonDB...\n");

        /// <summary>
        /// Clears the console window.
        /// </summary>
        public static void Clear() => Console.Clear();
    }
}