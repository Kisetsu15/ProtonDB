using Kisetsu.Utils;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ProtonDB {
    public static class ProtonMeta {
        private const string PROTON_DB = "ProtonDB";
        private static string DatabaseMetaFile => Path.Combine(DatabaseDirectory, Token.databaseMetaFile);
        private static string currentDatabase = Token.protonDB;

        public const string defaultDatabase = Token.protonDB;
        public static string CurrentDatabase { get => currentDatabase; set => currentDatabase = value; }
        public static string DatabaseDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), PROTON_DB, Token._database);
        public static Dictionary<string, string> GetDatabaseList() => (!File.Exists(DatabaseMetaFile)) ?
            [] : Json.Load<string>(Path.Combine(DatabaseDirectory, Token.databaseMetaFile));

        public static void Version() => Terminal.WriteLine($"{PROTON_DB} v{Assembly.GetExecutingAssembly().GetName().Version}");
        public static void Help() {

            Terminal.WriteLine("\nDatabase Operations: db.<operation>(argument)");
            Terminal.WriteLine("  use(name)                         Use a database");
            Terminal.WriteLine("  create(name)                      Create a new database");
            Terminal.WriteLine("  drop()                            Drop the current database");
            Terminal.WriteLine("  drop(name)                        Drop a specified database");
            Terminal.WriteLine("  list()                            List all databases");

            Terminal.WriteLine("\nCollection Operations: collection.<operation>(argument)");
            Terminal.WriteLine("  create(name)                      Create a new collection");
            Terminal.WriteLine("  drop(name)                        Drop a collection");
            Terminal.WriteLine("  list()                            List all collections in the current database");

            Terminal.WriteLine("\nDocument Operations: <collection_name>.<operation>(argument)");
            Terminal.WriteLine("  insert(data)                      Insert a document into collection");
            Terminal.WriteLine("  remove()                          Remove all documents in a collection");
            Terminal.WriteLine("  remove(condition)                 Remove documents matching the condition from collection");
            Terminal.WriteLine("  print()                           ASCIISplashScreen all documents in collection");
            Terminal.WriteLine("  print(condition)                  ASCIISplashScreen documents matching the condition from collection");
            Terminal.WriteLine("  update(action, data)              Update all documents in collection");
            Terminal.WriteLine("  update(action, data, condition)   Update documents matching the condition in collection");

            Terminal.WriteLine("\n  action    - [ add | drop | alter ]");
            Terminal.WriteLine("  data      -  {\"key\": value}");
            Terminal.WriteLine("  condition - key <operator> value");
            Terminal.WriteLine("  operators - [ < | <= | > | >= | = ]");
            Terminal.WriteLine("*Note*: data is {\"key\"} for update(drop, data, condition)");

            Terminal.WriteLine("\nCommands:");
            Terminal.WriteLine("  protondb                        Open ProtonDB");
            Terminal.WriteLine("  :h                             Show this help message");
            Terminal.WriteLine("  :v                             Show ProtonDB version");
            Terminal.WriteLine("  :q                             Exit ProtonDB\n");
        }


        public static void Initialize() {
            if (Directory.Exists(DatabaseDirectory)) return;

            Terminal.WriteLine($"Initializing {PROTON_DB}...");
            Directory.CreateDirectory(DatabaseDirectory);
            Terminal.WriteLine($"Creating database directory: {DatabaseDirectory}");
            Database.Create(Token.protonDB);
            AddToPath(Directory.GetCurrentDirectory());
            ASCIISplashScreen();
        }

        private static void AddToPath(string newPath) {
            try {
                var target = EnvironmentVariableTarget.User;
                var currentPath = Environment.GetEnvironmentVariable("PATH", target) ?? "";

                if (currentPath.Split(';').Contains(newPath)) {
                    Terminal.WriteLine($"'{newPath}' is already in PATH.", ConsoleColor.Yellow);
                    return;
                }

                var updatedPath = currentPath + ";" + newPath;
                Environment.SetEnvironmentVariable("PATH", updatedPath, target);
                Terminal.WriteLine($"Successfully added '{newPath}' to PATH.");
                RefreshEnvironment();
            } catch (Exception ex) {
                Terminal.WriteLine($"Failed to update PATH\nFatal error: {ex.Message}", ConsoleColor.Red);
            }
        }

        private static void RefreshEnvironment() {
            try {
                Terminal.WriteLine("Refreshing environment variables...");
                const int HWND_BROADCAST = 0xFFFF;
                const int WM_SETTINGCHANGE = 0x1A;
                const int SMTO_ABORTIFHUNG = 0x0002;

                SendMessageTimeout((IntPtr) HWND_BROADCAST, WM_SETTINGCHANGE, IntPtr.Zero, "Environment", SMTO_ABORTIFHUNG, 5000, out _);
            } catch (Exception ex) {
                Terminal.WriteLine($"Failed to refresh environment variables: {ex.Message}", ConsoleColor.Red);
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = false)]
        private static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint Msg, IntPtr wParam, string lParam, uint fuFlags, uint uTimeout, out IntPtr lpdwResult);

        private static void ASCIISplashScreen() {
            Terminal.WriteLine("\r\n\r\n                                                                      \r\n                               %...=*                                 \r\n                             :........%                               \r\n                           =...........+                              \r\n                          *.............-                             \r\n             #:....+%-   ................*    *#.....#:               \r\n           %............:%...............+@.............              \r\n          *.............#...#:........%.................+             \r\n          %............:.......%...#.......#.............             \r\n          +............#.........*#.....................=             \r\n           ...................*.....%.......%...........%             \r\n           #..........*.+#%%@%+=-:--=*@%%%#-+..........:              \r\n            *..*%-....%...#.............*.........%%....              \r\n           %.:........=.+.................%..=........*.%             \r\n       #...............:....................#%.......#.....*.         \r\n     *.........:.....%........@@@@@@.........%......#.........#       \r\n   :............*..:........@@@@@@@@@#.......#.%...-...........#      \r\n   :.............%%........:@@@@@@@@@@.......#..:-..............*     \r\n   *.............=-.........@@@@@@@@@@.......#..%.*..............     \r\n    -..........:....#.......*@@@@@@@@........#.:...%...........#      \r\n     *........+......-........:@@@@..........@......#........%        \r\n        %....+.........%...................+.#.......#....+-          \r\n            %.........+..%................:..-........@#              \r\n            .....=%%..%....#............+......=%#.....%              \r\n           %..........=......#.:=++=:.+.....#...........              \r\n           ............:.......%...+:.......*...........%             \r\n          #............%.........@:........:............:             \r\n          %...................*:....%......%.............             \r\n           .............%..#:..........%..:.............%             \r\n           :...........*#-................=%...........%              \r\n              +%%%#.      ...............%      *%%%#                 \r\n                           .............%                             \r\n                            ...........%                              \r\n                             %.......+                                \r\n                                %%%=                                  \r");
            Terminal.WriteLine("\r\n\r\n\t______          _               ____________ \r\n\t| ___ \\        | |              |  _  \\ ___ \\\r\n\t| |_/ / __ ___ | |_ ___  _ __   | | | | |_/ /\r\n\t|  __/ '__/ _ \\| __/ _ \\| '_ \\  | | | | ___ \\\r\n\t| |  | | | (_) | || (_) | | | | | |/ /| |_/ /\r\n\t\\_|  |_|  \\___/ \\__\\___/|_| |_| |___/ \\____/ \r\n\t                                             \r\n\t                                             \r\n\r\n");
        }
    }
}