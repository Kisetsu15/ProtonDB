namespace ProtonDB.Server {
    namespace Core {

        public static class Collection {
            public static string[] Create(string name) => Linker(name, StorageEngine.create_collection);

            public static string[] Drop(string name) => Linker(name, StorageEngine.drop_collection);

            public static string[] List(string name) {
                Result result = new();
                if (name == null || name == ProtonMeta.CurrentDatabase) {
                    result = StorageEngine.Link(
                        new QueryConfig {
                            databaseName = ProtonMeta.CurrentDatabase,
                        },
                        StorageEngine.list_collection
                    );

                    return result.GetOutput();
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

            private static string[] Linker(string name, Func<QueryConfig, Output> func) {
                if (string.IsNullOrEmpty(name)) {
                    return ["fatal: Collection name cannot be empty"];
                }
                Result result = StorageEngine.Link(
                    new QueryConfig {
                        databaseName = ProtonMeta.CurrentDatabase,
                        collectionName = name,
                    },
                    func
                );
                return result.GetOutput();
            }
        }
    }
}
