// -------------------------------------------------------------------------------------------------
//  File: Meta.cs
//  Namespace: ProtonDB.Server.Core
//  Description:
//      Provides static properties and methods for managing core metadata, directory paths, and
//      server logging for ProtonDB. Handles initialization of required directories and files,
//      manages database metadata, and provides logging utilities for server events and user actions.
//
//  Public Properties:
//      - ProtonDBDirectory: Path to the ProtonDB application data directory.
//      - CoreDirectory: Path to the core directory for server files.
//      - DatabaseDirectory: Path to the directory containing database files.
//      - AuthDirectory: Path to the authentication directory.
//      - ProfileConfig: Path to the user profile configuration file.
//      - defaultDatabase: Name of the default database (constant).
//      - maxMessageLength: Maximum allowed message length (constant).
//      - ServerLogs: Path to the server log file.
//
//  Public Methods:
//      - GetDatabaseList: Returns a dictionary of available databases and their metadata.
//      - Initialize: Ensures required directories and files exist; creates default database and admin profile if needed.
//      - Log: Overloaded methods for logging messages and user actions to the server log file.
//
//  Internal Methods:
//      - FileCheck: Ensures the server log file and required directories exist before logging.
//
//  Dependencies:
//      - QuerySession: Represents the current user session and context.
//      - Storage, Token, Profiles, Database: Provide constants and methods for storage, authentication, and database management.
//      - Kisetsu.Utils: Utility extensions (e.g., for file and path handling).
//
//  Usage Example:
//      Meta.Initialize(session);
//      Meta.Log("Server started");
//      var dbList = Meta.GetDatabaseList();
// -------------------------------------------------------------------------------------------------

using Kisetsu.Utils;

namespace ProtonDB.Server {
    namespace Core {

        /// <summary>
        /// Provides static properties and methods for managing core metadata, directory paths, and server logging.
        /// </summary>
        public static class Meta {
            /// <summary>
            /// Gets the path to the database metadata file.
            /// </summary>
            private static string DatabaseMetaFile => Path.Combine(DatabaseDirectory, Storage._databaseMeta);

            /// <summary>
            /// Gets the path to the ProtonDB application data directory.
            /// </summary>
            public static string ProtonDBDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Storage._protonDB);

            /// <summary>
            /// Gets the path to the core directory for server files.
            /// </summary>
            public static string CoreDirectory => Storage._coreDir;

            /// <summary>
            /// Gets the path to the directory containing database files.
            /// </summary>
            public static string DatabaseDirectory => Path.Combine(ProtonDBDirectory, Token._database);

            /// <summary>
            /// Gets the path to the authentication directory.
            /// </summary>
            public static string AuthDirectory => Path.Combine(CoreDirectory, Storage._authDir);

            /// <summary>
            /// Gets the path to the user profile configuration file.
            /// </summary>
            public static string ProfileConfig => Path.Combine(AuthDirectory, Storage._profileConfig);

            /// <summary>
            /// The name of the default database.
            /// </summary>
            public const string defaultDatabase = Token.proton;

            /// <summary>
            /// The maximum allowed message length.
            /// </summary>
            public const int maxMessageLength = 384;

            /// <summary>
            /// Gets the path to the server log file.
            /// </summary>
            public static string ServerLogs => Path.Combine(CoreDirectory, "server.log");

            /// <summary>
            /// Returns a dictionary of available databases and their metadata.
            /// </summary>
            /// <returns>Dictionary of database names and metadata.</returns>
            public static Dictionary<string, string> GetDatabaseList() => (!File.Exists(DatabaseMetaFile)) ?
                [] : Json.Load<string>(Path.Combine(DatabaseDirectory, Storage._databaseMeta));

            /// <summary>
            /// Ensures required directories and files exist; creates default database and admin profile if needed.
            /// </summary>
            /// <param name="session">Optional query session for context and initialization.</param>
            public static void Initialize(QuerySession? session = null) {
                if (!Directory.Exists(DatabaseDirectory) && session != null) {
                    Directory.CreateDirectory(DatabaseDirectory);
                    Database.Create(Token.proton, session);
                }
                if (!Directory.Exists(CoreDirectory) || !Directory.Exists(AuthDirectory)) {
                    Directory.CreateDirectory(AuthDirectory);
                    Profiles.Admin("admin123", "welcome");
                    File.SetAttributes(CoreDirectory, FileAttributes.Hidden);
                }
            }

            /// <summary>
            /// Logs a message to the server log file with a timestamp.
            /// </summary>
            /// <param name="message">The message to log.</param>
            /// <returns>The logged message.</returns>
            public static string Log(string message) {
                FileCheck();
                File.AppendAllText(ServerLogs, $"{DateTime.UtcNow:o} : {message}\n");
                return message;
            }

            /// <summary>
            /// Logs a message to the server log file with a timestamp and user context.
            /// </summary>
            /// <param name="message">The message to log.</param>
            /// <param name="session">The current query session.</param>
            /// <returns>The logged message.</returns>
            public static string Log(string message, QuerySession session) {
                FileCheck();
                File.AppendAllText(ServerLogs, $"{DateTime.UtcNow:o} {session.CurrentUser} : {message}\n");
                return message;
            }

            /// <summary>
            /// Logs multiple messages to the server log file with a timestamp and user context.
            /// </summary>
            /// <param name="message">The messages to log.</param>
            /// <param name="session">The current query session.</param>
            /// <returns>The logged messages.</returns>
            public static string[] Log(string[] message, QuerySession session) {
                FileCheck();
                File.AppendAllText(ServerLogs, $"{DateTime.UtcNow:o} {session.CurrentUser}:\n");
                File.AppendAllLines(ServerLogs, message);
                File.AppendAllText(ServerLogs, "\n");
                return message;
            }

            /// <summary>
            /// Ensures the server log file and required directories exist before logging.
            /// </summary>
            private static void FileCheck() {
                if (!File.Exists(ServerLogs)) {
                    if (Directory.Exists(CoreDirectory)) {
                        File.Create(ServerLogs);
                    } else {
                        Directory.CreateDirectory(AuthDirectory);
                        File.Create(ServerLogs);
                    }
                }
            }
        }
    }
}