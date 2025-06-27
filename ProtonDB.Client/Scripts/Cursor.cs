using System.Reflection;
using Newtonsoft.Json;

namespace ProtonDB.Client{
    public class Cursor(Connection connection) : IDisposable {
        private readonly ProtonSession _session = connection.Session;

        public void Query(string query) {
            var response = _session.QueryAsync(query).GetAwaiter().GetResult();
            if (response.Status != "ok") {
                throw new Exception($"Query failed: {response.Message}");
            }
        }

        public void SafeQuery(string query) {
            try {
                Query(query);
            } catch (Exception) {
                connection.Reconnect();
                Query(query);
            }
        }

        public string QueryRaw(string query) {
            var res = _session.QueryAsync(query).GetAwaiter().GetResult();
            return res.Message;
        }


        public string[] FetchAll() {
            var response = _session.FetchAsync().GetAwaiter().GetResult();
            return (response.Result == null || response.Result.Length == 0) ? [] : response.Result;
        }

        public T? Map<T>(int index = 0) {
            var result = FetchOne(index);
            if (string.IsNullOrWhiteSpace(result)) return default;

            try {
                return JsonConvert.DeserializeObject<T>(result);
            } catch {
                return default;
            }
        }

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

        public (string raw, T? mapped) MapWithRaw<T>(int index = 0) {
            var result = FetchOne(index);
            T? obj = string.IsNullOrEmpty(result) ? default : JsonConvert.DeserializeObject<T>(result);
            return (result, obj);
        }


        public string FetchOne(int index = 0) {
            var result = FetchAll();
            return (index > result.Length || result.Length == 0) ? string.Empty : result[index];
        }

        public void Debug(bool enable) {
            var response = _session.DebugAsync(enable).GetAwaiter().GetResult();
            if (response.Status != "ok") {
                throw new Exception($"Debug command failed: {response.Message}");
            }
        }

        public string Quit() {
            var response = _session.QuitAsync().GetAwaiter().GetResult();
            if (response.Status != "ok") {
                throw new Exception($"Quit command failed: {response.Message}");
            } else {
                _session.Dispose();
                return response.Message;
            }
        }

        public string Version() => $"ProtonDB v{Assembly.GetExecutingAssembly().GetName().Version}";

        public void Dispose() => _session.Dispose();
    }
}
