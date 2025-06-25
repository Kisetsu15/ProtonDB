using Kisetsu.Utils;

namespace ProtonDB.CORE {
    public static class Database {
        public static void Use(string name) {
            if (string.IsNullOrEmpty(name)) {
                Terminal.WriteLine("Database name cannot be empty");
                return;
            }

            if (name == ProtonMeta.CurrentDatabase) {
                Terminal.WriteLine($"Already using database: {name}");
                return;
            }

            ProtonMeta.CurrentDatabase = name;
            if (!ProtonMeta.GetDatabaseList().ContainsKey(name)) StorageEngine.CreateDatabase(name);
            Terminal.WriteLine($"Switched to database: {name}");
        }

        public static void Create(string name) => StorageEngine.CreateDatabase(name);

        public static void Drop(string name) {
            if ((name == null && ProtonMeta.CurrentDatabase == ProtonMeta.defaultDatabase) || name == ProtonMeta.defaultDatabase) {
                Terminal.WriteLine("fatal: Cannot drop the default database");
                return;
            }

            if (name == null || name == ProtonMeta.CurrentDatabase) {
                StorageEngine.DropDatabase(ProtonMeta.CurrentDatabase);
                ProtonMeta.CurrentDatabase = ProtonMeta.defaultDatabase;
                return;
            }

            StorageEngine.DropDatabase(name);

        }

        public static void List() => StorageEngine.ListDatabase();
    }
}
