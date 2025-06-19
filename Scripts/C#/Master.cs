using Kisetsu.Utils;
using System.Text;

namespace MicroDB {

    public static class Master {

        public static string currentDatabase = "db";
        public static Dictionary<string, string> databases = Json.Load<string>(Path.Combine(Directory.GetCurrentDirectory(), "db",".database.meta"));

        public static void Main(string[] args) {
            while (true) {
                string input = Terminal.Input($"{currentDatabase}> ");
                Parser.Execute(input);
            }

        }

        
    }
}
