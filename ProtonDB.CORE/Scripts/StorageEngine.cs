using System.Runtime.InteropServices;

namespace ProtonDB.CORE {

    public enum Condition {
        greaterThan,
        greaterThanEqual,
        lessThan,
        lessThanEqual,
        equal,
        all,
        invalid,
    };

    public enum Action {
        add,
        drop,
        alter,
        invalid,
    };

    public static class StorageEngine {
        [DllImport("libProtonDB.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void CreateDatabase(string databaseName);

        [DllImport("libProtonDB.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DeleteDatabase(string databaseName);

        [DllImport("libProtonDB.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ListDatabase();

        [DllImport("libProtonDB.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void CreateCollection(string databaseName, string collectionName);

        [DllImport("libProtonDB.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DeleteCollection(string databaseName, string collectionName);

        [DllImport("libProtonDB.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ListCollection(string databaseName);

        [DllImport("libProtonDB.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void InsertDocument(string databaseName, string collectionName, string document);

        [DllImport("libProtonDB.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DeleteDocuments(string databaseName, string collectionName, string key, string value, Condition condition);

        [DllImport("libProtonDB.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DeleteAllDocuments(string databaseName, string collectionName);

        [DllImport("libProtonDB.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void PrintAllDocuments(string databaseName, string collectionName);

        [DllImport("libProtonDB.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void PrintDocuments(string databaseName, string collectionName, string key, string value, Condition condition);

        [DllImport("libProtonDB.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void UpdateAllDocuments(string databaseName, string collectionName, Action action, string param);

        [DllImport("libProtonDB.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void UpdateDocuments(string databaseName, string collectionName, string key, string value, Condition condition, Action action, string param);

    }
}