using System.Text.Json;

namespace ProtonDB.Server {
    public static class ConfigLoader {
        static readonly JsonSerializerOptions _jsonOptions = new() {
            WriteIndented = true
        };

        public static ServerConfig Load(string filePath = "config.json") {
            if (!File.Exists(filePath)) {
                var defaultConfig = new ServerConfig();
                var options = _jsonOptions;
                File.WriteAllText(filePath, JsonSerializer.Serialize(defaultConfig, options));
                Console.WriteLine("Config file not found. Generated default config.");
                return defaultConfig;
            }

            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<ServerConfig>(json)!;
        }
    }
}