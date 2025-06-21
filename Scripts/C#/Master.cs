using Kisetsu.Utils;

namespace ProtonDB {

    public static class Master {

        public const string defaultDatabase = Token.protonDB;
        public static string CurrentDatabase { get => currentDatabase ; set => currentDatabase = value; }
        public static Dictionary<string, string> Databases { get; private set; } = Json.Load<string>(Path.Combine(DatabaseDirectory, Token.databaseMetaFile));
        private static string DatabaseDirectory => Path.Combine(Directory.GetCurrentDirectory(), Token._database);
        private static string currentDatabase = Token.protonDB;

        public static void Main() {

            InitializeEnvironment();
            while (true) {
                string input = Terminal.Input($"{CurrentDatabase}> ");
                if (string.IsNullOrWhiteSpace(input)) continue;
                if (input.Equals(Token._quit, StringComparison.OrdinalIgnoreCase)) {
                    Terminal.WriteLine("Exiting ProtonDB...");
                    break;
                }
                if (input.Equals(Token._help, StringComparison.OrdinalIgnoreCase)) {
                    ShowHelp();
                    continue;
                }
                Parser.Execute(input);
            }
        }

        private static void InitializeEnvironment() {
            if (!Directory.Exists(DatabaseDirectory)) {
                Directory.CreateDirectory(DatabaseDirectory);
                Database.Create(Token.protonDB);
            }
            Databases = Json.Load<string>(Path.Combine(DatabaseDirectory, Token.databaseMetaFile));
        }

        private static void ShowHelp() {

            Terminal.WriteLine("\nDatabase Operations: db.<operation>(argument)");
            Terminal.WriteLine("  use(name)                         Use a database");
            Terminal.WriteLine("  create(name)                      Create a new database");
            Terminal.WriteLine("  drop()                            Drop the current database");
            Terminal.WriteLine("  drop(name)                        Drop a specified database");
            Terminal.WriteLine("  list()                            List all databases");

            Terminal.WriteLine("\nCollection Operations: collection.<operation>(argument)");         
            Terminal.WriteLine("  create(name)                      Create a new collection");
            Terminal.WriteLine("  drop(name)                        Drop a collection");
            Terminal.WriteLine("  list()                            List all collections in the current database");

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
            Terminal.WriteLine("*Note*: data is {\"key\"} for update(drop, data, condition)");

            Terminal.WriteLine("\nCommands:");
            Terminal.WriteLine("  protondb                        Open ProtonDB");
            Terminal.WriteLine("  :h                             Show this help message");
            Terminal.WriteLine("  :q                             Exit ProtonDB\n");
        }
    }
}
