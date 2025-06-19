using System.Runtime.InteropServices;

namespace MicroDB {

    public enum Condition {
        greaterThan,
        greaterThanEqual,
        lessThan,
        lessThanEqual,
        equal,
        all,
    };

    public enum Action {
        add,
        drop,
        alter
    };

    public static class StorageEngine {
        [DllImport("libMicroDB.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void CreateDatabase(string databaseName);

        [DllImport("libMicroDB.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DeleteDatabase(string databaseName);

        [DllImport("libMicroDB.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ListDatabase();

        [DllImport("libMicroDB.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void CreateCollection(string databaseName, string collectionName);

        [DllImport("libMicroDB.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DeleteCollection(string databaseName, string collectionName);

        [DllImport("libMicroDB.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ListCollection(string databaseName);

        [DllImport("libMicroDB.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void InsertDocument(string databaseName, string collectionName, string document);

        [DllImport("libMicroDB.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DeleteDocuments(string databaseName, string collectionName, string key, string value, Condition condition);

        [DllImport("libMicroDB.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DeleteAllDocuments(string databaseName, string collectionName);

        [DllImport("libMicroDB.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void PrintAllDocuments(string databaseName, string collectionName);

        [DllImport("libMicroDB.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void PrintDocuments(string databaseName, string collectionName, string key, string value, Condition condition);

        [DllImport("libMicroDB.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void UpdateAllDocuments(string databaseName, string collectionName, Action action, string param);

        [DllImport("libMicroDB.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void UpdateDocuments(string databaseName, string collectionName, string key, string value, Condition condition, Action action, string param);

    }
}