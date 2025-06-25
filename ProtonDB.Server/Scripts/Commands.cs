using Kisetsu.Utils;
using System.Reflection;

namespace ProtonDB.Server {
    namespace Core {

        public static class Commands {
            public static void Version() => Terminal.WriteLine($" ProtonDB v{Assembly.GetExecutingAssembly().GetName().Version}");
            public static void Help() {
                Terminal.WriteLine("\nUsage: protondb <command>");
                Terminal.WriteLine("\nCommands:");
                Terminal.WriteLine("  protondb                          Open ProtonDB");
                Terminal.WriteLine("  :h, --help                        Show this help message");
                Terminal.WriteLine("  :v, --version                     Show ProtonDB version");
                Terminal.WriteLine("  :q, quit                          Exit ProtonDB");
                Terminal.WriteLine("  cls                               Clear Console");
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
                Terminal.WriteLine("*Note*: data is {\"key\"} for update(drop, data, condition)\n");

            }

            public static void Clear() => Console.Clear();
        }
    }
}