// -------------------------------------------------------------------------------------------------
//  File: StorageEngine.cs
//  Namespace: ProtonDB.Server.Core
//  Description:
//      Provides interop bindings and utility methods for interacting with the native storage
//      engine (libStorageEngine.dll) used by ProtonDB. Defines configuration, result, and
//      output structures for marshaling data between managed and unmanaged code. Exposes
//      methods for database, collection, and document operations, as well as helpers for
//      result handling and memory management.
//
//  Public Enums:
//      - Condition: Specifies comparison operators for queries (e.g., equal, greaterThan).
//      - Action: Specifies actions for document updates (add, drop, alter).
//
//  Public Structs:
//      - QueryConfig: Configuration for storage engine operations (database, collection, key, etc.).
//      - Result: Encapsulates the result of a storage operation, including success, data, and error.
//      - Output: Marshaled output from native storage engine functions (single result).
//      - ArrayOut: Marshaled output for array results from native storage engine functions.
//
//  Public Methods:
//      - Link: Executes a storage engine operation and returns a Result (overloads for Output/ArrayOut).
//      - ListDatabase: Returns a list of all databases from the storage engine.
//
//  Native Methods (DllImport):
//      - create_database, drop_database, list_database, create_collection, drop_collection, list_collection
//      - insert_document, remove_all_documents, remove_documents, print_all_documents, print_documents
//      - update_all_documents, update_documents, free_list
//
//  Internal Methods:
//      - GetArray: Converts unmanaged array pointers to managed string arrays.
//
//  Dependencies:
//      - Kisetsu.Utils: Utility extensions.
//      - System.Runtime.InteropServices: For interop and marshaling.
//
//  Usage Example:
//      var result = StorageEngine.Link(config, StorageEngine.create_database);
//      var collections = StorageEngine.Link(config, StorageEngine.list_collection);
// -------------------------------------------------------------------------------------------------

using System.Runtime.InteropServices;

namespace ProtonDB.Server {
    namespace Core {

        /// <summary>
        /// Specifies comparison operators for queries.
        /// </summary>
        public enum Condition {
            greaterThan,
            greaterThanEqual,
            lessThan,
            lessThanEqual,
            equal,
            notEqual,
            all,
            invalid,
        };

        /// <summary>
        /// Specifies actions for document updates.
        /// </summary>
        public enum Action {
            add,
            drop,
            alter,
            invalid,
        };

        /// <summary>
        /// Configuration for storage engine operations.
        /// </summary>
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

        /// <summary>
        /// Encapsulates the result of a storage operation.
        /// </summary>
        public struct Result {
            public bool success;
            public string[] data;
            public string? error;

            /// <summary>
            /// Returns the output data or error message, optionally appending a custom message.
            /// </summary>
            /// <param name="message">Optional message to append.</param>
            /// <returns>Array of result strings.</returns>
            public readonly string[] GetOutput(string? message = null) {
                if (!success) return [error!];

                if (message != null) {
                    return data.Length > 0 ? [data[0] + "\n" + message] : [message];
                }
                return data;
            }
        }

        /// <summary>
        /// Marshaled output from native storage engine functions (single result).
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Output {
            public int success;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 384)]
            public string? message;
        }

        /// <summary>
        /// Marshaled output for array results from native storage engine functions.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct ArrayOut {
            public int size;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 384)]
            public string? message;
            public IntPtr list;
        }

        /// <summary>
        /// Provides interop bindings and utility methods for the native storage engine.
        /// </summary>
        public static class StorageEngine {
            private const string STORAGE_ENGINE_PATH = "libStorageEngine.dll";
            private static readonly Result nullConfig = new() {
                success = false,
                data = [],
                error = "Database or collection name cannot be null"
            };

            /// <summary>
            /// Executes a storage engine operation returning Output and wraps the result.
            /// </summary>
            /// <param name="config">The query configuration.</param>
            /// <param name="func">The storage engine function to call.</param>
            /// <returns>A Result object with operation outcome.</returns>
            public static Result Link(QueryConfig config, Func<QueryConfig, Output> func) {
                if ((config.databaseName == null && config.collectionName == null) || config.databaseName == null) return nullConfig;
                var output = func(config);
                return new Result {
                    success = output.success == 1,
                    data = [output.message!],
                    error = output.success == 1 ? null : output.message
                };
            }

            /// <summary>
            /// Executes a storage engine operation returning ArrayOut and wraps the result.
            /// </summary>
            /// <param name="config">The query configuration.</param>
            /// <param name="func">The storage engine function to call.</param>
            /// <returns>A Result object with operation outcome.</returns>
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

            /// <summary>
            /// Returns a list of all databases from the storage engine.
            /// </summary>
            /// <returns>Array of database names or error message.</returns>
            public static string[] ListDatabase() {
                var arrayOut = list_database();
                string[] data = arrayOut.size > 0 && arrayOut.list != IntPtr.Zero
                    ? GetArray(arrayOut.list, arrayOut.size)
                    : [];

                return arrayOut.size > 0 ? data : [arrayOut.message ?? "null"];
            }

            /// <summary>
            /// Converts an unmanaged array pointer to a managed string array and frees native memory.
            /// </summary>
            /// <param name="arrPtr">Pointer to the unmanaged array.</param>
            /// <param name="size">Number of elements in the array.</param>
            /// <returns>Managed string array.</returns>
            private static string[] GetArray(IntPtr arrPtr, int size) {
                string[] array = new string[size];
                for (int i = 0; i < size; i++) {
                    IntPtr ptr = Marshal.ReadIntPtr(arrPtr, i * IntPtr.Size);
                    array[i] = Marshal.PtrToStringAnsi(ptr) ?? string.Empty;
                }
                free_list(arrPtr, size);
                return array;
            }

            // Native storage engine function bindings
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