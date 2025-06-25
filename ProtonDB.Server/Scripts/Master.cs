using Kisetsu.Utils;
using System.Text;

namespace ProtonDB.Server {

    enum Operation {
        Skip,
        End,
        Noop,
    }

    public static class Master {

        private const int MAX_LENGTH = 4096;
        
        public static void Main(string[] args) {
            ProtonMeta.Initialize();

            while (true) {
                string input = Terminal.Input($"{ProtonMeta.CurrentDatabase}> ");
                input = MultiLineParser(input);
                Parser.Execute(input);
            }
        }

        private static string MultiLineParser(string input) {
            var sb = new StringBuilder();
            input = input.Trim();
            sb.Append(input);
            
            while (true) {
                if (sb.Length > MAX_LENGTH) {
                    Console.WriteLine("MultiLineParser too long.");
                    return string.Empty;
                }

                if (!string.IsNullOrWhiteSpace(input) && input.EndsWith(')')) break;

                input = Terminal.Input($"{ProtonMeta.CurrentDatabase}= ");
                if (string.IsNullOrWhiteSpace(input)) continue;

                sb.Append(input.Trim());
            }

            return sb.ToString().Trim();
        }

    }
}
