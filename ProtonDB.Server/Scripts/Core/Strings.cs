// -------------------------------------------------------------------------------------------------
//  File: Strings.cs
//  Namespace: ProtonDB.Server.Core
//  Description:
//      Defines static classes containing string constants used throughout the ProtonDB server
//      for command tokens, entity names, storage configuration, and privilege levels. These
//      constants provide a centralized, type-safe way to reference protocol keywords and
//      configuration identifiers, reducing the risk of typos and improving maintainability.
//
//  Public Static Classes:
//      - Token: Command and object tokens for query parsing and dispatch (e.g., "db", "create", "drop").
//      - Command: Server command keywords for client-server communication (e.g., "LOGIN", "QUERY").
//      - Entity: Entity field names used in query parsing (e.g., "object", "operation", "argument").
//      - Storage: File and directory names for storage configuration and metadata.
//      - Privilege: User privilege level identifiers ("user", "admin").
//
//  Usage Example:
//      if (query.Operation == Token.create) { ... }
//      var configPath = Storage._profileConfig;
//      if (user.Privilege == Privilege.admin) { ... }
// -------------------------------------------------------------------------------------------------

namespace ProtonDB.Server {
    namespace Core {
        /// <summary>
        /// Contains string constants for command and object tokens used in query parsing and dispatch.
        /// </summary>
        public static class Token {
            public const string _database = "db";
            public const string collection = "collection";
            public const string profile = "profile";
            public const string use = "use";

            public const string create = "create";
            public const string drop = "drop";
            public const string list = "list";
            public const string insert = "insert";
            public const string remove = "remove";
            public const string update = "update";
            public const string print = "print";
            public const string grant = "grant";
            public const string revoke = "revoke";
            public const string delete = "delete";

            public const string proton = "proton";
        }

        /// <summary>
        /// Contains string constants for server command keywords used in client-server communication.
        /// </summary>
        public static class Command {
            public const string login = "LOGIN";
            public const string fetch = "FETCH";
            public const string query = "QUERY";
            public const string quit = "QUIT";
            public const string debug = "DEBUG";
            public const string profile = "PROFILE";
        }

        /// <summary>
        /// Contains string constants for entity field names used in query parsing.
        /// </summary>
        public static class Entity {
            public const string @object = "object";
            public const string operation = "operation";
            public const string argument = "argument";
        }

        /// <summary>
        /// Contains string constants for file and directory names used in storage configuration and metadata.
        /// </summary>
        public static class Storage {
            public const string _databaseMeta = ".database.meta";
            public const string _protonDB = "ProtonDB";
            public const string _coreDir = ".core";
            public const string _authDir = "auth";
            public const string _profileConfig = "profileConfig.json";
        }

        /// <summary>
        /// Contains string constants for user privilege level identifiers.
        /// </summary>
        public static class Privilege {
            public const string user = "user";
            public const string admin = "admin";
        }
    }
}