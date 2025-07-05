using Kisetsu.Utils;
using System.Reflection;

namespace ProtonDB.Shell {
    public static class Commands {
        public static void Version() => Terminal.WriteLine($" ProtonDB v{Assembly.GetExecutingAssembly().GetName().Version}\n");
        public static void Help() {
            Terminal.WriteLine("\nUsage: protondb <command>");
            Terminal.WriteLine("\nCommands:");
            Terminal.WriteLine("  protondb                                      Open ProtonDB");
            Terminal.WriteLine("  :h, --help                                    Show this help message");
            Terminal.WriteLine("  :v, --version                                 Show ProtonDB version");
            Terminal.WriteLine("  :q, quit                                      Exit ProtonDB");
            Terminal.WriteLine("  cls                                           Clear Console");

            Terminal.WriteLine("\nProfile Operations: profile.<operation>(argument)");
            Terminal.WriteLine("  create(<username>, <password>, <privilege>)   Create a new profile");
            Terminal.WriteLine("  delete(<username>)                            Drop a profiles");
            Terminal.WriteLine("  grant(<username>, <database>)                 Grant access to specified database");
            Terminal.WriteLine("  grant(<username>)                             Grant access to current database");
            Terminal.WriteLine("  revoke(<username>, <database>)                Revoke access to specified database");
            Terminal.WriteLine("  revoke(<username>)                            Revoke access to current database");
            Terminal.WriteLine("  list()                                        List all profiles");

            Terminal.WriteLine("\nDatabase Operations: db.<operation>(argument)");
            Terminal.WriteLine("  use(<database>)                               Use a database");
            Terminal.WriteLine("  create(<database>)                            Create a new database");
            Terminal.WriteLine("  drop()                                        Drop the current database");
            Terminal.WriteLine("  drop(<database>)                              Drop a specified database");
            Terminal.WriteLine("  list()                                        List all databases");

            Terminal.WriteLine("\nCollection Operations: collection.<operation>(argument)");
            Terminal.WriteLine("  create(<collection>)                          Create a new collection");
            Terminal.WriteLine("  drop(<collection>)                            Drop a collection");
            Terminal.WriteLine("  list()                                        List all collections in the current database");

            Terminal.WriteLine("\nDocument Operations: <collection>.<operation>(argument)");
            Terminal.WriteLine("  insert(data)                                  Insert a document into collection");
            Terminal.WriteLine("  insert([<data1>, <data2>, ...])               Insert multiple documents into collection");
            Terminal.WriteLine("  remove()                                      Remove all documents in a collection");
            Terminal.WriteLine("  remove(<condition>)                           Remove documents matching the condition from collection");
            Terminal.WriteLine("  print()                                       Print all documents in collection");
            Terminal.WriteLine("  print(<condition>)                            Print documents matching the condition from collection");
            Terminal.WriteLine("  update(<action>, <data>)                      Update all documents in collection");
            Terminal.WriteLine("  update(<action>, <data>, <condition>)         Update documents matching the condition in collection");

            Terminal.WriteLine("\n  action    - [ add | drop | alter ]");
            Terminal.WriteLine("  data      -  {\"key\": value}");
            Terminal.WriteLine("  condition - key <operator> value");
            Terminal.WriteLine("  operators - [ < | <= | > | >= | = | != ]");
            Terminal.WriteLine("*Note*: data is {\"key\"} for update(drop, data, condition)\n");

        }

        public static void Quit() => Terminal.WriteLine("Exiting ProtonDB...\n");

        public static void Clear() => Console.Clear();
    }
}