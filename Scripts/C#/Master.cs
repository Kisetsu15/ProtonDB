using Kisetsu.Utils;

namespace MicroDB {

    public static class Master {

        public static string CurrentDatabase { get; private set; } = Token._database;
        private static string DatabaseDirectory => Path.Combine(Directory.GetCurrentDirectory(), "db");
        public static Dictionary<string, string> Databases { get; private set; } = Json.Load<string>(Path.Combine(DatabaseDirectory, ".database.meta"));


        public static void Main(string[] args) {
            if (!Directory.Exists(DatabaseDirectory)) {
                Directory.CreateDirectory(DatabaseDirectory);
                Database.Create(Token._database);
            }
            while (true) {
                string input = Terminal.Input($"{CurrentDatabase}> ");
                Parser.Execute(input);
            }

        }

        public static void SetDatabase(string name) => CurrentDatabase = name;

    }
}
