namespace ProtonDB.Server {
    namespace Core {
        public static class Token {
            // Query tokens
            public const string _database = "db";
            public const string collection = "collection";
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
            public const string guest = "guest";
            public const string user = "user";
            public const string admin = "admin";
            public const string password = "password";
            public const string privilege = "privilege";
            public const string created_at = "created_at";
            public const string database = "database";
            public const string salt = "salt";



            // File tokens
            public const string _databaseMetaFile = ".database.meta";
            public const string _protonDB = "ProtonDB";
            public const string _coreDir = ".core";
            public const string _authDir = "auth";
            public const string _userConfigFile = "userConfig.json";

            // Operation tokens
            public const string protonDB = "protonDB";
            public const string _help = ":h";
            public const string _quit = ":q";
            public const string quit = "quit";
            public const string _version = ":v";
            public const string version = "--version";
            public const string help = "--help";
            public const string clear = "cls";

        }
    }
}