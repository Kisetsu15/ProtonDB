using Kisetsu.Utils;
using System.Runtime.InteropServices;

namespace ProtonDB.Server {
    namespace Core {

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

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct QueryConfig {
            public string? databaseName;
            public string? collectionName;
            public string? key;
            public string? value;
            public string? data;
            public Condition condition;
            public Action action;
        }

        public struct Result {
            public bool success;
            public string[] data;
            public string? error;
            public readonly string[] GetOutput(string? message = null) {
                if (!success) return [error!];

                if (message != null) {
                    return data.Length > 0 ? [data[0] + "\n" + message] : [message];
                }

                return data;
            }
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct Output {
            public bool success;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 384)]
            public string? message;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ArrayOut {
            public int size; 
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 384)]
            public string? message;
            public IntPtr list;
        }

        public static class StorageEngine {
            private const string STORAGE_ENGINE_PATH = "libStorageEngine.dll";
            private static readonly Result nullConfig = new() {
                success = false,
                data = [],
                error = "Database or collection name cannot be null"
            };


            public static Result Link(QueryConfig config, Func<QueryConfig, Output> func) {
                if ((config.databaseName == null && config.collectionName == null) || config.databaseName == null) return nullConfig;
                var output = func(config);
                return new Result {
                    success = output.success,
                    data = [output.message!],
                    error = output.success ? null : output.message
                };
            }

            public static Result Link(QueryConfig config, Func<QueryConfig, ArrayOut> func) {
                if ((config.databaseName == null && config.collectionName == null) || config.databaseName == null) return nullConfig;
                ArrayOut arrayOut = func(config);
                string[] data = arrayOut.size > 0 && arrayOut.list != IntPtr.Zero
                    ? GetArray(arrayOut.list, arrayOut.size)
                    : [];
                return new Result {
                    success = arrayOut.size > 0,
                    data = data,
                    error = arrayOut.size > 0 ? null : arrayOut.message
                };
            }

            public static string[] ListDatabase() {
                var arrayOut = list_database();
                string[] data = arrayOut.size > 0 && arrayOut.list != IntPtr.Zero
                    ? GetArray(arrayOut.list, arrayOut.size)
                    : [];

                return arrayOut.size > 0 ? data : [arrayOut.message ?? "null"];
            }


            private static string[] GetArray(IntPtr arrPtr, int size) {
                string[] array = new string[size];
                for (int i = 0; i < size; i++) {
                    IntPtr ptr = Marshal.ReadIntPtr(arrPtr, i * IntPtr.Size);
                    array[i] = Marshal.PtrToStringAnsi(ptr) ?? string.Empty;
                }
                free_list(arrPtr, size);
                return array;
            }

            [DllImport(STORAGE_ENGINE_PATH, CallingConvention = CallingConvention.Cdecl)]
            public static extern Output create_database(QueryConfig queryConfig);
            [DllImport(STORAGE_ENGINE_PATH, CallingConvention = CallingConvention.Cdecl)]
            public static extern Output drop_database(QueryConfig queryConfig);
            [DllImport(STORAGE_ENGINE_PATH, CallingConvention = CallingConvention.Cdecl)]
            public static extern ArrayOut list_database();
            [DllImport(STORAGE_ENGINE_PATH, CallingConvention = CallingConvention.Cdecl)]
            public static extern Output create_collection(QueryConfig queryConfig);
            [DllImport(STORAGE_ENGINE_PATH, CallingConvention = CallingConvention.Cdecl)]
            public static extern Output drop_collection(QueryConfig queryConfig);
            [DllImport(STORAGE_ENGINE_PATH, CallingConvention = CallingConvention.Cdecl)]
            public static extern ArrayOut list_collection(QueryConfig queryConfig);
            [DllImport(STORAGE_ENGINE_PATH, CallingConvention = CallingConvention.Cdecl)]
            public static extern Output insert_document(QueryConfig queryConfig);
            [DllImport(STORAGE_ENGINE_PATH, CallingConvention = CallingConvention.Cdecl)]
            public static extern Output remove_all_documents(QueryConfig queryConfig);
            [DllImport(STORAGE_ENGINE_PATH, CallingConvention = CallingConvention.Cdecl)]
            public static extern Output remove_documents(QueryConfig queryConfig);
            [DllImport(STORAGE_ENGINE_PATH, CallingConvention = CallingConvention.Cdecl)]
            public static extern ArrayOut print_all_documents(QueryConfig queryConfig);
            [DllImport(STORAGE_ENGINE_PATH, CallingConvention = CallingConvention.Cdecl)]
            public static extern ArrayOut print_documents(QueryConfig queryConfig);
            [DllImport(STORAGE_ENGINE_PATH, CallingConvention = CallingConvention.Cdecl)]
            public static extern Output update_all_documents(QueryConfig queryConfig);
            [DllImport(STORAGE_ENGINE_PATH, CallingConvention = CallingConvention.Cdecl)]
            public static extern Output update_documents(QueryConfig queryConfig);
            [DllImport(STORAGE_ENGINE_PATH, CallingConvention = CallingConvention.Cdecl)]
            private static extern void free_list(IntPtr list, int size);
        }
    }
}