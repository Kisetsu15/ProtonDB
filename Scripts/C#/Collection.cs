namespace MicroDB {
    public static class Collection {
        public static void Create(string name) => StorageEngine.CreateCollection(Master.currentDatabase, name);
        public static void Drop(string name) => StorageEngine.DeleteCollection(Master.currentDatabase, name);
        public static void List() => StorageEngine.ListCollection(Master.currentDatabase);
    }
}
