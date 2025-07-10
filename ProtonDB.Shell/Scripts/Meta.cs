using Kisetsu.Utils;
using System.Runtime.InteropServices;

namespace ProtonDB.Shell {
    public static class Meta {
        private const string PROTON_DB = "ProtonDB";

        public static void Loading() {
            if (AddToPath(AppContext.BaseDirectory)) {
                ASCIISplashScreen();
                Console.WriteLine($"\nWelcome to {PROTON_DB}!");
            }
        }

        private static bool AddToPath(string newPath) {
            try {
                var target = EnvironmentVariableTarget.User;
                var currentPath = Environment.GetEnvironmentVariable("PATH", target) ?? "";

                if (currentPath.Split(';').Contains(newPath)) {
                    return false;
                }

                var updatedPath = currentPath + ";" + newPath;
                Environment.SetEnvironmentVariable("PATH", updatedPath, target);
                Terminal.WriteLine($"\rSuccessfully added '{newPath}' to PATH");
                RefreshEnvironment();
                return true;
            } catch (Exception ex) {
                Terminal.WriteLine($"\rFailed to update PATH\nFatal error: {ex.Message}", ConsoleColor.Red);
                return false;
            }
        }

        private static void RefreshEnvironment() {
            try {
                Terminal.WriteLine("\rRefreshing environment variables...");
                const int HWND_BROADCAST = 0xFFFF;
                const int WM_SETTINGCHANGE = 0x1A;
                const int SMTO_ABORTIFHUNG = 0x0002;

                SendMessageTimeout((IntPtr) HWND_BROADCAST, WM_SETTINGCHANGE, IntPtr.Zero, "Environment", SMTO_ABORTIFHUNG, 5000, out _);
            } catch (Exception ex) {
                Terminal.WriteLine($"\rFailed to refresh environment variables: {ex.Message}", ConsoleColor.Red);
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