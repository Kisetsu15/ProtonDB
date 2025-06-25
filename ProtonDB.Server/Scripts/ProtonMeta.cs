using Kisetsu.Utils;

namespace ProtonDB.Server {
    public static class ProtonMeta {
        private const string PROTON_DB = "ProtonDB";
        private static string DatabaseMetaFile => Path.Combine(DatabaseDirectory, Token.databaseMetaFile);
        private static string currentDatabase = Token.protonDB;

        public const string defaultDatabase = Token.protonDB;
        public const int maxMessageLength = 384;
        public static string CurrentDatabase { get => currentDatabase; set => currentDatabase = value; }
        public static string DatabaseDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), PROTON_DB, Token._database);
        public static Dictionary<string, string> GetDatabaseList() => (!File.Exists(DatabaseMetaFile)) ?
            [] : Json.Load<string>(Path.Combine(DatabaseDirectory, Token.databaseMetaFile));


        public static void Initialize() {
            if (Directory.Exists(DatabaseDirectory)) return;
            Directory.CreateDirectory(DatabaseDirectory);
            Database.Create(Token.protonDB);
        }
    }
}