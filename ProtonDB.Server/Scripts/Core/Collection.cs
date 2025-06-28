namespace ProtonDB.Server {
    namespace Core {
        public static class Collection {
            public static string[] Create(string name, QuerySession session) => Linker(name, StorageEngine.create_collection, session);

            public static string[] Drop(string name, QuerySession session) => Linker(name, StorageEngine.drop_collection, session);

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

                if (Profiles.ValidateAccess(name, session)) {
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
