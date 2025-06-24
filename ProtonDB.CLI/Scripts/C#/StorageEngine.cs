using System.Runtime.InteropServices;
using Kisetsu.Utils;

namespace ProtonDB.CLI {

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

        private const string storageEnginePath = "libProtonDB.CLI.dll";

        [DllImport(storageEnginePath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void CreateDatabase(string databaseName);

        [DllImport(storageEnginePath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void DeleteDatabase(string databaseName);

        [DllImport(storageEnginePath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ListDatabase();

        [DllImport(storageEnginePath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void CreateCollection(string databaseName, string collectionName);

        [DllImport(storageEnginePath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void DeleteCollection(string databaseName, string collectionName);

        [DllImport(storageEnginePath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ListCollection(string databaseName);

        [DllImport(storageEnginePath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void InsertDocument(string databaseName, string collectionName, string document);

        [DllImport(storageEnginePath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void DeleteDocuments(string databaseName, string collectionName, string key, string value, Condition condition);

        [DllImport(storageEnginePath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void DeleteAllDocuments(string databaseName, string collectionName);

        [DllImport(storageEnginePath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void PrintAllDocuments(string databaseName, string collectionName);

        [DllImport(storageEnginePath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void PrintDocuments(string databaseName, string collectionName, string key, string value, Condition condition);

        [DllImport(storageEnginePath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void UpdateAllDocuments(string databaseName, string collectionName, Action action, string param);

        [DllImport(storageEnginePath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void UpdateDocuments(string databaseName, string collectionName, string key, string value, Condition condition, Action action, string param);

    }
}