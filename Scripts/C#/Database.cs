using Kisetsu.Utils;
using System.ComponentModel;

namespace MicroDB {
    public static class Database {
        public static void Use(string name) {
            if (string.IsNullOrEmpty(name)) {
                Terminal.WriteLine("Database name cannot be empty");
                return;
            }

            if (name == Master.CurrentDatabase) {
                Terminal.WriteLine($"Already using database: {name}");
                return;
            }

            Master.SetDatabase(name);
            if (!Master.Databases.ContainsKey(name)) StorageEngine.CreateDatabase(name);
        }

        public static void Create(string name) => StorageEngine.CreateDatabase(name);

        public static void Drop(string name) {
            if (name == null && name == Token._database) {
                Terminal.WriteLine("Cannot drop the default database", ConsoleColor.Red);
                return;
            }

            if (name == null || name == Master.CurrentDatabase) {
                StorageEngine.DeleteDatabase(Master.CurrentDatabase);
                Master.SetDatabase(Token._database);
                return;
            }

            StorageEngine.DeleteDatabase(name);

        }

        public static void List() => StorageEngine.ListDatabase();
    }
}
