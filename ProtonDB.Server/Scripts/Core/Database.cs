// -------------------------------------------------------------------------------------------------
//  File: Database.cs
//  Namespace: ProtonDB.Server.Core
//  Description:
//      Provides static methods for database management operations such as switching databases,
//      creating, dropping, and listing databases. Enforces access control and session context
//      for all operations. Integrates with Profiles for access validation and StorageEngine
//      for low-level database actions.
//
//  Public Methods:
//      - Use: Switches the current session to the specified database, creating it if necessary.
//      - Create: Creates a new database and updates profile metadata.
//      - Drop: Drops an existing database, with protection for the default database.
//      - List: Lists all available databases.
//
//  Internal Methods:
//      - Linker: Helper to standardize StorageEngine calls and result formatting.
//
//  Dependencies:
//      - QuerySession: Represents the current user session and context.
//      - Profiles: Handles access validation and profile updates.
//      - StorageEngine: Executes database operations.
//      - Meta: Provides metadata and default database information.
//      - QueryConfig, Result, Output: Data structures for query configuration and results.
//
//  Usage Example:
//      var result = Database.Use("myDatabase", session);
// -------------------------------------------------------------------------------------------------

namespace ProtonDB.Server {
    namespace Core {
        /// <summary>
        /// Provides static methods for managing databases, including switching, creating, dropping, and listing.
        /// </summary>
        public static class Database {
            /// <summary>
            /// Switches the current session to the specified database.
            /// If the database does not exist, it is created.
            /// </summary>
            /// <param name="name">The name of the database to use.</param>
            /// <param name="session">The current query session.</param>
            /// <returns>Status or error messages.</returns>
            public static string[] Use(string name, QuerySession session) {
                if (string.IsNullOrEmpty(name)) return ["Database name cannot be empty"];
                if (name == session.CurrentDatabase) return [$"Already using database '{name}'"];
                if (!Profiles.ValidateAccess(name, session)) return ["Access denied to the database"];

                if (!Meta.GetDatabaseList().ContainsKey(name)) {
                    return Linker(name, StorageEngine.create_database, $"Switched to database: {name}");
                }

                session.CurrentDatabase = name;
                return [$"Switched to database: {name}"];
            }

            /// <summary>
            /// Creates a new database and updates profile metadata.
            /// </summary>
            /// <param name="name">The name of the database to create.</param>
            /// <param name="session">The current query session.</param>
            /// <returns>Status or error messages.</returns>
            public static string[] Create(string name, QuerySession session) {
                string[] result = Linker(name, StorageEngine.create_database);
                Profiles.UpdateDatabase(name, Action.add, session);
                Profiles.UpdateAdminDatabase();
                return result;
            }

            /// <summary>
            /// Drops an existing database, with protection for the default database.
            /// Updates profile metadata accordingly.
            /// </summary>
            /// <param name="name">The name of the database to drop, or null to drop the current database.</param>
            /// <param name="session">The current query session.</param>
            /// <returns>Status or error messages.</returns>
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

            /// <summary>
            /// Helper to standardize StorageEngine calls and result formatting.
            /// </summary>
            /// <param name="name">The database name.</param>
            /// <param name="func">The StorageEngine function to execute.</param>
            /// <param name="message">Optional message to include in the result.</param>
            /// <returns>Status or error messages.</returns>
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

            /// <summary>
            /// Lists all available databases. Does not accept arguments.
            /// </summary>
            /// <param name="args">Arguments (should be empty).</param>
            /// <returns>List of database names or error messages.</returns>
            public static string[] List(string args) {
                if (!string.IsNullOrEmpty(args)) return ["List doesn't support argument"];
                var result = StorageEngine.ListDatabase();
                if (result.Length == 0) {
                    return ["No databases found"];
                }
                return result;
            }
        }
    }
}