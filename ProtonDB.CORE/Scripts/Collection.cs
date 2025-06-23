namespace ProtonDB.CORE {
    public static class Collection {
        public static void Create(string name) => StorageEngine.CreateCollection(ProtonMeta.CurrentDatabase, name);

        public static void Drop(string name) => StorageEngine.DeleteCollection(ProtonMeta.CurrentDatabase, name);
        
        public static void List() => StorageEngine.ListCollection(ProtonMeta.CurrentDatabase);
    }
}
