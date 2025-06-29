using Kisetsu.Utils;

namespace ProtonDB.Server {
    namespace Core {


        public static class Meta {
            private static string DatabaseMetaFile => Path.Combine(DatabaseDirectory, Storage._databaseMeta);
            public static string ProtonDBDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Storage._protonDB);
            public static string CoreDirectory => Storage._coreDir;
            public static string DatabaseDirectory => Path.Combine(ProtonDBDirectory, Token._database);
            public static string AuthDirectory => Path.Combine(CoreDirectory, Storage._authDir);
            public static string ProfileConfig => Path.Combine(AuthDirectory, Storage._profileConfig);

            public const string defaultDatabase = Token.proton;
            public const int maxMessageLength = 384;
            public static Dictionary<string, string> GetDatabaseList() => (!File.Exists(DatabaseMetaFile)) ?
                [] : Json.Load<string>(Path.Combine(DatabaseDirectory, Storage._databaseMeta));


            public static void Initialize(QuerySession session) {
                if (!Directory.Exists(DatabaseDirectory)) {
                    Directory.CreateDirectory(DatabaseDirectory);
                    Database.Create(Token.proton, session);
                }
                if (!Directory.Exists(CoreDirectory)) {
                    Directory.CreateDirectory(AuthDirectory);
                    Profiles.Admin("admin123", "welcome");
                    File.SetAttributes(CoreDirectory, FileAttributes.Hidden);
                }
            }
        }
    }
}