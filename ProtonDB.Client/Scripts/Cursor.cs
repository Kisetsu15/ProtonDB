using Newtonsoft.Json;

namespace ProtonDB.Client {
    /// <summary>
    /// Provides methods to execute queries and fetch results from a ProtonDB session.
    /// </summary>
    public class Cursor(Connection connection) : IDisposable {
        private readonly ProtonSession _session = connection.Session;

        /// <summary>
        /// Executes a query synchronously. Throws an exception if the query fails.
        /// </summary>
        /// <param name="query">The query string to execute.</param>
        /// <exception cref="Exception">Thrown if the query response status is not "ok".</exception>
        public void Query(string query) {
            var response = _session.QueryAsync(query).GetAwaiter().GetResult();
            if (response.Status != "ok") {
                throw new Exception($"Query failed: {response.Message}");
            }
        }

        /// <summary>
        /// Executes a query and attempts to reconnect and retry if the first attempt fails.
        /// </summary>
        /// <param name="query">The query string to execute.</param>
        public void SafeQuery(string query) {
            try {
                Query(query);
            } catch (Exception) {
                connection.Reconnect();
                Query(query);
            }
        }

        /// <summary>
        /// Executes a query and returns the raw response message.
        /// </summary>
        /// <param name="query">The query string to execute.</param>
        /// <returns>The raw response message.</returns>
        public string QueryRaw(string query) {
            var res = _session.QueryAsync(query).GetAwaiter().GetResult();
            return res.Message;
        }

        /// <summary>
        /// Fetches all results from the last query.
        /// </summary>
        /// <returns>An array of result strings, or an empty array if no results.</returns>
        public string[] FetchAll() {
            var response = _session.FetchAsync().GetAwaiter().GetResult();
            return (response.Result == null || response.Result.Length == 0) ? [] : response.Result;
        }

        /// <summary>
        /// Maps a single result to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to map the result to.</typeparam>
        /// <param name="index">The index of the result to map (default is 0).</param>
        /// <returns>The mapped object, or default if mapping fails.</returns>
        public T? Map<T>(int index = 0) {
            var result = FetchOne(index);
            if (string.IsNullOrWhiteSpace(result)) return default;

            try {
                return JsonConvert.DeserializeObject<T>(result);
            } catch {
                return default;
            }
        }

        /// <summary>
        /// Maps all results to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to map the results to.</typeparam>
        /// <returns>An array of mapped objects.</returns>
        public T[] MapAll<T>() {
            var result = FetchAll();
            return result
                .Select(item => {
                    try {
                        return JsonConvert.DeserializeObject<T>(item)!;
                    } catch {
                        return default!;
                    }
                })
                .Where(x => x != null)
                .ToArray()!;
        }

        /// <summary>
        /// Fetches a single result and maps it to the specified type, returning both the raw and mapped values.
        /// </summary>
        /// <typeparam name="T">The type to map the result to.</typeparam>
        /// <param name="index">The index of the result to fetch (default is 0).</param>
        /// <returns>A tuple containing the raw result and the mapped object.</returns>
        public (string raw, T? mapped) MapWithRaw<T>(int index = 0) {
            var result = FetchOne(index);
            T? obj = string.IsNullOrEmpty(result) ? default : JsonConvert.DeserializeObject<T>(result);
            return (result, obj);
        }

        /// <summary>
        /// Fetches a single result by index.
        /// </summary>
        /// <param name="index">The index of the result to fetch (default is 0).</param>
        /// <returns>The result string, or an empty string if not found.</returns>
        public string FetchOne(int index = 0) {
            var result = FetchAll();
            return (index > result.Length || result.Length == 0) ? string.Empty : result[index];
        }

        /// <summary>
        /// Enables or disables debug mode on the session.
        /// </summary>
        /// <param name="enable">True to enable debug mode, false to disable.</param>
        /// <exception cref="Exception">Thrown if the debug command fails.</exception>
        public void Debug(bool enable) {
            var response = _session.DebugAsync(enable).GetAwaiter().GetResult();
            if (response.Status != "ok") {
                throw new Exception($"Debug command failed: {response.Message}");
            }
        }

        /// <summary>
        /// Fetches details about the current profile.
        /// </summary>
        /// <returns>An array of result strings, or an empty array if no results.</returns>
        public string[] Profile() {
            var response = _session.ProfileAsync().GetAwaiter().GetResult();
            if (response.Status != "ok") {
                throw new Exception($"Profile command failed: {response.Message}");
            }
            return response.Result ?? [];
        }

        /// <summary>
        /// Quits the session and disposes resources.
        /// </summary>
        /// <returns>The quit response message.</returns>
        /// <exception cref="Exception">Thrown if the quit command fails.</exception>
        public string Quit() {
            var response = _session.QuitAsync().GetAwaiter().GetResult();
            if (response.Status != "ok") {
                throw new Exception($"Quit command failed: {response.Message}");
            } else {
                _session.Dispose();
                return response.Message;
            }
        }

        /// <summary>
        /// Disposes the underlying session.
        /// </summary>
        public void Dispose() => _session.Dispose();
    }
}