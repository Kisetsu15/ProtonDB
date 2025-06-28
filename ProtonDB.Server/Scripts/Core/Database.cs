namespace ProtonDB.Server {
    namespace Core {
        public static class Database {
            public static string[] Use(string name, QuerySession session) {
                if (string.IsNullOrEmpty(name))     return ["Database name cannot be empty"];
                if (name == session.CurrentDatabase)   return [$"Already using database '{name}'"];
                if (!Profiles.ValidateAccess(name, session)) return ["Access denied to the database"];

                if (!Meta.GetDatabaseList().ContainsKey(name)) {
                    return Linker(name, StorageEngine.create_database, $"Switched to database: {name}");
                }

                session.CurrentDatabase = name;
                return [$"Switched to database: {name}"];
            }


            public static string[] Create(string name, QuerySession session) {
                string[] result = Linker(name, StorageEngine.create_database);
                foreach (string s in result) {
                    Console.WriteLine(s);
                }
                Profiles.UpdateDatabase(name, Action.add, session);
                Profiles.UpdateAdminDatabase();
                return result; 
            }

            public static string[] Drop(string name, QuerySession session) {
                if ((name == null && session.CurrentDatabase == Meta.defaultDatabase) || name == Meta.defaultDatabase) {
                    return ["fatal: Cannot drop the default database"];
                }

                if (name == null || name == session.CurrentDatabase) {
                    Profiles.UpdateDatabase(session.CurrentDatabase, Action.drop, session);
                    return Linker(session.CurrentDatabase, StorageEngine.drop_database);
                }
                string[] result = Linker(name, StorageEngine.drop_database);
                Profiles.UpdateDatabase(session.CurrentDatabase, Action.drop, session);
                Profiles.UpdateAdminDatabase();
                return result;
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
}
