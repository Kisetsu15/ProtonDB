using Kisetsu.Utils;

namespace MicroDB {
    public static class Database {
        public static void Use(string name) {
            Master.currentDatabase = name;
            if (!Master.databases.ContainsKey(name)) StorageEngine.CreateDatabase(name);
        }
        public static void Create(string name) => StorageEngine.CreateDatabase(name);
        public static void Drop(string name) {
            if (name == null && name == Clause._database) {
                Terminal.WriteLine("Cannot drop the default database", ConsoleColor.Red);
                return;
            }

            if (name == null || name == Master.currentDatabase) {
                StorageEngine.DeleteDatabase(Master.currentDatabase);
                Master.currentDatabase = Clause._database;
                return;
            }

            StorageEngine.DeleteDatabase(name);

        }
        public static void List() => StorageEngine.ListDatabase();
    }
}
