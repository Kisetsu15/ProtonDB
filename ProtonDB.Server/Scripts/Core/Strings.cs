namespace ProtonDB.Server {
    namespace Core {
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

        public static class Command {
            public const string login = "LOGIN";
            public const string fetch = "FETCH";
            public const string query = "QUERY";
            public const string quit = "QUIT";
            public const string debug = "DEBUG";
        }

        public static class Entity {
            public const string @object = "object";
            public const string operation = "operation";
            public const string argument = "argument";
        }

        public static class Storage {
            public const string _databaseMeta = ".database.meta";
            public const string _protonDB = "ProtonDB";
            public const string _coreDir = ".core";
            public const string _authDir = "auth";
            public const string _profileConfig = "profileConfig.json";
        }

        public static class Privilege {
            public const string guest = "guest";
            public const string user = "user";
            public const string admin = "admin";
        }
    }
}