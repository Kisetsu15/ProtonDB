using System.Reflection;

namespace ProtonDB.Client{
    public class Cursor(Connection connection) {
        private readonly ProtonDBSession _session = connection.Session;

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
            if (response.Result == null || response.Result.Length == 0) {
                return [];
            }
            return response.Result;
        }

        public string FetchOne() {
            var response = _session.FetchAsync().GetAwaiter().GetResult();
            if (response.Result == null || response.Result.Length == 0) {
                return string.Empty;
            }
            return response.Result[0];
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
    }
}
