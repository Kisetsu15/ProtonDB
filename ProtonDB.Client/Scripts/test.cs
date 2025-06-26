using Kisetsu.Utils;
namespace ProtonDB.Client {
    public class Test {
        public static void Main(string[] args) {
            using var session = new ProtonDBSession();
            session.Query("db.list()");
            var results = session.FetchAll();
            foreach (var result in results) {
                Console.WriteLine(result);
            }
        }
    }
}