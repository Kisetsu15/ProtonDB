// -------------------------------------------------------------------------------------------------
//  File: Collection.cs
//  Namespace: ProtonDB.Server.Core
//  Description:
//      Provides static methods for managing collections within a database context, including
//      creation, deletion, and listing of collections. Methods interact with the StorageEngine
//      and enforce access control via Profiles. All operations require a QuerySession to
//      maintain context and permissions.
//
//  Public Methods:
//      - Create: Creates a new collection in the current database.
//      - Drop: Drops an existing collection from the current database.
//      - List: Lists collections in the specified or current database, with access validation.
//
//  Internal Methods:
//      - Linker: Helper method to invoke StorageEngine operations with standardized error handling.
//
//  Dependencies:
//      - QuerySession: Represents the current user session and context.
//      - StorageEngine: Handles low-level collection operations.
//      - Profiles: Validates user access to databases.
//      - QueryConfig, Result, Output: Data structures for query configuration and results.
//
//  Usage Example:
//      var collections = Collection.List("myDatabase", session);
// -------------------------------------------------------------------------------------------------

namespace ProtonDB.Server {
    namespace Core {
        /// <summary>
        /// Provides static methods for managing collections in the database.
        /// </summary>
        public static class Collection {
            /// <summary>
            /// Creates a new collection in the current database.
            /// </summary>
            /// <param name="name">The name of the collection to create.</param>
            /// <param name="session">The current query session.</param>
            /// <returns>Result messages from the operation.</returns>
            public static string[] Create(string name, QuerySession session) => Linker(name, StorageEngine.create_collection, session);

            /// <summary>
            /// Drops an existing collection from the current database.
            /// </summary>
            /// <param name="name">The name of the collection to drop.</param>
            /// <param name="session">The current query session.</param>
            /// <returns>Result messages from the operation.</returns>
            public static string[] Drop(string name, QuerySession session) => Linker(name, StorageEngine.drop_collection, session);

            /// <summary>
            /// Lists collections in the specified or current database.
            /// Validates access permissions before listing.
            /// </summary>
            /// <param name="name">The database name, or null for the current database.</param>
            /// <param name="session">The current query session.</param>
            /// <returns>List of collection names or error messages.</returns>
            public static string[] List(string name, QuerySession session) {
                Result result = new();
                if (name == null || name == session.CurrentDatabase) {
                    result = StorageEngine.Link(
                        new QueryConfig {
                            databaseName = session.CurrentDatabase,
                        },
                        StorageEngine.list_collection
                    );

                    return result.GetOutput();
                }

                if (!Profiles.ValidateAccess(name, session)) {
                    return ["Access denied to the database"];
                }

                result = StorageEngine.Link(
                    new QueryConfig {
                        databaseName = name,
                    },
                    StorageEngine.list_collection
                );

                if (!result.success) return [result.error!];

                return (result.data.Length == 0) ? ["No collection found"] : result.data;
            }

            /// <summary>
            /// Helper method to invoke StorageEngine operations with standardized error handling.
            /// </summary>
            /// <param name="name">The collection name.</param>
            /// <param name="func">The StorageEngine function to execute.</param>
            /// <param name="session">The current query session.</param>
            /// <returns>Result messages from the operation.</returns>
            private static string[] Linker(string name, Func<QueryConfig, Output> func, QuerySession session) {
                if (string.IsNullOrEmpty(name)) {
                    return ["fatal: Collection name cannot be empty"];
                }
                Result result = StorageEngine.Link(
                    new QueryConfig {
                        databaseName = session.CurrentDatabase,
                        collectionName = name,
                    },
                    func
                );
                return result.GetOutput();
            }
        }
    }
}