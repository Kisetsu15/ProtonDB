using Kisetsu.Utils;
namespace ProtonDB.Client {
    public class Test {
        public static void Main(string[] args) {
            var con = Connection.Connect(Connection.defaultHost, Connection.defaultPort, "admin123", "welcome");
            if (!con.IsConnected) {
                Console.WriteLine("Failed to connect to the server.");
                return;
            }
            var cursor = new Cursor(con);
            cursor.Debug(true);
            
            var results = cursor.FetchAll();
            foreach (var result in results) {
                Console.WriteLine(result);
            }
            cursor.Query("profile.list()");
            results = cursor.FetchAll();
            foreach (var result in results) {
               Console.WriteLine(result);
            }
            cursor.Quit();
            
        }
    }
}