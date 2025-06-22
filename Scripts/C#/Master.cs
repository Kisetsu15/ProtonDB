using Kisetsu.Utils;

namespace ProtonDB {

    public static class Master {

        public static void Main() {

            InitializeEnvironment();
            while (true) {
                string input = Terminal.Input($"{ProtonMeta.CurrentDatabase}> ");
                if (string.IsNullOrWhiteSpace(input)) continue;
                if (input.Equals(Token._quit, StringComparison.OrdinalIgnoreCase)) {
                    Terminal.WriteLine("Exiting ProtonDB...");
                    break;
                }
                if (input.Equals(Token._help, StringComparison.OrdinalIgnoreCase)) {
                    ProtonMeta.Help();
                    continue;
                }
                if (input.Equals(Token._version, StringComparison.OrdinalIgnoreCase)) {
                    ProtonMeta.Version();
                    continue;
                }
                Parser.Execute(input);
            }
        }

        private static void InitializeEnvironment() {
            if (!Directory.Exists(ProtonMeta.DatabaseDirectory)) {
                Directory.CreateDirectory(ProtonMeta.DatabaseDirectory);
                Database.Create(Token.protonDB);
            }
        }

        
    }
}
