namespace ProtonDB.Server {
    namespace Core {
        public static class Token {
            // Query tokens
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

            // Object tokens
            public const string @object = "object";
            public const string operation = "operation";
            public const string argument = "argument";
            public const string action = "action";
            public const string data = "data";
            public const string condition = "condition";



            // File tokens
            public const string _databaseMeta = ".database.meta";
            public const string _protonDB = "ProtonDB";
            public const string _coreDir = ".core";
            public const string _authDir = "auth";
            public const string _profileConfig = "profileConfig.json";

            // Operation tokens
            public const string proton = "proton";

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