// -------------------------------------------------------------------------------------------------
//  File: ConfigLoader.cs
//  Namespace: ProtonDB.Server
//  Description:
//      Provides static methods for loading and initializing the server configuration from a JSON
//      file. If the configuration file does not exist, a default ServerConfig instance is created,
//      serialized, and saved to disk. The configuration is deserialized using System.Text.Json
//      with indented formatting for readability.
//
//  Public Methods:
//      - Load: Loads the server configuration from the specified file path. If the file does not
//        exist, generates and saves a default configuration.
//
//  Usage Example:
//      var config = ConfigLoader.Load(); // Loads from "serverConfig.json" by default
//
//  Dependencies:
//      - ServerConfig: Represents the server's configuration (host, port, debug, max connections).
//      - System.Text.Json: Used for serialization and deserialization.
// -------------------------------------------------------------------------------------------------

using System.Text.Json;

namespace ProtonDB.Server {
    /// <summary>
    /// Provides methods for loading and initializing the server configuration from a JSON file.
    /// </summary>
    public static class ConfigLoader {
        static readonly JsonSerializerOptions _jsonOptions = new() {
            WriteIndented = true
        };

        /// <summary>
        /// Loads the server configuration from the specified file path.
        /// If the file does not exist, creates and saves a default configuration.
        /// </summary>
        /// <param name="filePath">The path to the configuration file (default: "serverConfig.json").</param>
        /// <returns>The loaded or default <see cref="ServerConfig"/> instance.</returns>
        public static ServerConfig Load(string filePath = "serverConfig.json") {
            if (!File.Exists(filePath)) {
                var defaultConfig = new ServerConfig();
                var options = _jsonOptions;
                File.WriteAllText(filePath, JsonSerializer.Serialize(defaultConfig, options));
                Console.WriteLine("Server config file not found. Generated default config.");
                return defaultConfig;
            }

            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<ServerConfig>(json)!;
        }
    }
}