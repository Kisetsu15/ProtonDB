using Kisetsu.Utils;
using System;
using System.Xml.Linq;

namespace ProtonDB.Server {
    public static class Database {
        public static string[] Use(string name) {
            if (string.IsNullOrEmpty(name)) {
                return ["Database name cannot be empty"];
            }

            if (name == ProtonMeta.CurrentDatabase) {
                return [$"Already using database '{name}'"];
            }

            if (!ProtonMeta.GetDatabaseList().ContainsKey(name)) {
                return Linker(name, StorageEngine.create_database, $"Switched to database: {name}");
            }

            ProtonMeta.CurrentDatabase = name;
            return [$"Switched to database: {name}"];
        }


        public static string[] Create(string name) => Linker(name, StorageEngine.create_database);

        public static string[] Drop(string name) {
            if ((name == null && ProtonMeta.CurrentDatabase == ProtonMeta.defaultDatabase) || name == ProtonMeta.defaultDatabase) {
                return ["fatal: Cannot drop the default database"];
            }

            if (name == null || name == ProtonMeta.CurrentDatabase) {
                return Linker(ProtonMeta.CurrentDatabase, StorageEngine.drop_database);
            }

            return Linker(name, StorageEngine.drop_database);
        }

        private static string[] Linker(string name, Func<QueryConfig, Output> func, string? message = null) {
            if (string.IsNullOrEmpty(name)) {
                return ["fatal: Database name cannot be empty"];
            }
            Result result = StorageEngine.Link(
                new QueryConfig {
                    databaseName = name,
                },
                func
            );
            return result.GetOutput(message);
        }

        public static string[] List() {
            var result = StorageEngine.ListDatabase();
            if (result.Length == 0) {
                return ["No databases found"];
            }
            return result;
        }
    }
}
