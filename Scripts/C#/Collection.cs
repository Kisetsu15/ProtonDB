namespace ProtonDB {
    public static class Collection {
        public static void Create(string name) => StorageEngine.CreateCollection(Master.CurrentDatabase, name);

        public static void Drop(string name) => StorageEngine.DeleteCollection(Master.CurrentDatabase, name);
        
        public static void List() => StorageEngine.ListCollection(Master.CurrentDatabase);
    }
}
