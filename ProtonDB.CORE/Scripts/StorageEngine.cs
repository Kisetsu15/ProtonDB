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
        private const string STORAGE_ENGINE_PATH = "libStorageEngine.dll";

        public static string CreateDatabase(string databaseName) {
            IntPtr ptr = create_database(databaseName);
            return Marshal.PtrToStringAnsi(ptr)!;
        }

        public static string DropDatabase(string databaseName) {
            IntPtr ptr = drop_database(databaseName);
            return Marshal.PtrToStringAnsi(ptr)!;
        }

        public static string[] ListDatabase() {
            IntPtr arrayPtr = list_database(out int count);
            string[] result = new string[count];

            for (int i = 0; i < count; i++) {
                IntPtr strPtr = Marshal.ReadIntPtr(arrayPtr, i * IntPtr.Size);
                result[i] = Marshal.PtrToStringAnsi(strPtr)!;
            }
            free_list(arrayPtr);
            return result;
        }

        public static string CreateCollection(string databaseName, string collectionName) {
            IntPtr ptr = create_collection(databaseName, collectionName);
            return Marshal.PtrToStringAnsi(ptr)!;
        }

        public static string DropCollection(string databaseName, string collectionName) {
            IntPtr ptr = drop_collection(databaseName, collectionName);
            return Marshal.PtrToStringAnsi(ptr)!;
        }

        public static string[] ListCollection(string datbaseName) {
            IntPtr arrayPtr = list_collection(datbaseName, out int count);
            string[] result = new string[count];
            for (int i = 0; i < count; i++) {
                IntPtr strPtr = Marshal.ReadIntPtr(arrayPtr, i * IntPtr.Size);
                result[i] = Marshal.PtrToStringAnsi(strPtr)!;
            }
            free_list(arrayPtr);
            return result;
        }

        public static string InsertDocument(string databaseName, string collectionName, string document) {
            IntPtr ptr = insert_document(databaseName, collectionName, document);
            return Marshal.PtrToStringAnsi(ptr)!;
        }

        public static string RemoveAllDocuments(string databaseName, string collectionName) {
            IntPtr ptr = remove_all_documents(databaseName, collectionName);
            return Marshal.PtrToStringAnsi(ptr)!;
        }

        public static string RemoveDocuments(string databaseName, string collectionName, string key, string value, Condition condition) {
            IntPtr ptr = remove_documents(databaseName, collectionName, key, value, condition);
            return Marshal.PtrToStringAnsi(ptr)!;
        }

        public static string[] PrintAllDocuments(string databaseName, string collectionName) {
            IntPtr arrayPtr = print_all_documents(databaseName, collectionName, out IntPtr messagePtr, out int count);
            string[]? documents = new string[count];
            for (int i = 0; i < count; i++) {
                IntPtr strPtr = Marshal.ReadIntPtr(arrayPtr, i * IntPtr.Size);
                documents[i] = Marshal.PtrToStringAnsi(strPtr)!;
            }
            string message = Marshal.PtrToStringAnsi(messagePtr)!;
            free_list(arrayPtr);
            return (count < 0) ? [message] : documents;
        }

        public static string[] PrintDocuments(string databaseName, string collectionName, string key, string value, Condition condition) {
            IntPtr arrayPtr = print_documents(databaseName, collectionName, key, value, condition, out IntPtr messagePtr, out int count);
            string[]? documents = new string[count];
            for (int i = 0; i < count; i++) {
                IntPtr strPtr = Marshal.ReadIntPtr(arrayPtr, i * IntPtr.Size);
                documents[i] = Marshal.PtrToStringAnsi(strPtr)!;
            }
            string message = Marshal.PtrToStringAnsi(messagePtr)!;
            free_list(arrayPtr);
            return (count < 0) ? [message] : documents;
        }

        public static string UpdateAllDocuments(string databaseName, string collectionName, Action action, string data) {
            IntPtr ptr = update_all_documents(databaseName, collectionName, action, data);
            return Marshal.PtrToStringAnsi(ptr)!;
        }

        public static string UpdateDocuments(string databaseName, string collectionName, string key, string value, Condition condition, Action action, string data) {
            IntPtr ptr = update_documents(databaseName, collectionName, key, value, condition, action, data);
            return Marshal.PtrToStringAnsi(ptr)!;
        }



        [DllImport(STORAGE_ENGINE_PATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr create_database(string databaseName);
        [DllImport(STORAGE_ENGINE_PATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr drop_database(string databaseName);
        [DllImport(STORAGE_ENGINE_PATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr list_database(out int count);
        [DllImport(STORAGE_ENGINE_PATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr create_collection(string databaseName, string collectionName);
        [DllImport(STORAGE_ENGINE_PATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr drop_collection(string databaseName, string collectionName);
        [DllImport(STORAGE_ENGINE_PATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr list_collection(string databaseName, out int count);
        [DllImport(STORAGE_ENGINE_PATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr insert_document(string databaseName, string collectionName, string document);
        [DllImport(STORAGE_ENGINE_PATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr remove_all_documents(string databaseName, string collectionName);
        [DllImport(STORAGE_ENGINE_PATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr remove_documents(string databaseName, string collectionName, string key, string value, Condition condition);
        [DllImport(STORAGE_ENGINE_PATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr print_all_documents(string databaseName, string collectionName, out IntPtr message, out int count);
        [DllImport(STORAGE_ENGINE_PATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr print_documents(string databaseName, string collectionName, string key, string value, Condition condition, out IntPtr message, out int count);
        [DllImport(STORAGE_ENGINE_PATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr update_all_documents(string databaseName, string collectionName, Action action, string data);
        [DllImport(STORAGE_ENGINE_PATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr update_documents(string databaseName, string collectionName, string key, string value, Condition condition, Action action, string data);
        [DllImport(STORAGE_ENGINE_PATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern void free_list(IntPtr list);
    }
}