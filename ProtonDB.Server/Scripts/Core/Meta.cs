using Kisetsu.Utils;

namespace ProtonDB.Server {
    namespace Core {
        public static class Meta {
            private static string currentDatabase = Token.protonDB;
            private static string currentUser = AES.GenerateSalt();
            private static string currentPrivalege = Token.guest;
            public static string CurrentUser { get => currentUser; set => currentUser = value; }
            public static string CurrentPrivilege { get => currentPrivalege; set => currentPrivalege = value; }
            public static string ProtonDBDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Token._protonDB);
            public static string CoreDirectory => Path.Combine(ProtonDBDirectory, Token._coreDir);
            public static string DatabaseDirectory => Path.Combine(ProtonDBDirectory, Token._database);
            private static string DatabaseMetaFile => Path.Combine(DatabaseDirectory, Token._databaseMetaFile);
            public static string AuthDirectory => Path.Combine(CoreDirectory, Token._authDir);
            public static string UserConfigFile => Path.Combine(AuthDirectory, Token._userConfigFile);

            public const string defaultDatabase = Token.protonDB;
            public const int maxMessageLength = 384;
            public static string CurrentDatabase { get => currentDatabase; set => currentDatabase = value; }
            public static Dictionary<string, string> GetDatabaseList() => (!File.Exists(DatabaseMetaFile)) ?
                [] : Json.Load<string>(Path.Combine(DatabaseDirectory, Token._databaseMetaFile));


            public static void Initialize() {
                if (!Directory.Exists(DatabaseDirectory)) {
                    Directory.CreateDirectory(DatabaseDirectory);
                    Database.Create(Token.protonDB);
                }
                if (!Directory.Exists(CoreDirectory)) {
                    Directory.CreateDirectory(AuthDirectory);
                    File.WriteAllText(UserConfigFile, "{}");
                    
                    File.SetAttributes(CoreDirectory, FileAttributes.Hidden);
                }
            }
        }
    }
}