// -------------------------------------------------------------------------------------------------
//  File: Meta.cs
//  Namespace: ProtonDB.Shell
//  Description:
//      Provides meta-operations for the ProtonDB shell, such as environment setup and splash screen
//      display. This class is responsible for initializing the shell environment, ensuring the
//      application's directory is added to the user's PATH, displaying an ASCII splash screen, and
//      broadcasting environment variable changes to the system.
//
//  Public Methods:
//      - Loading: Initializes the ProtonDB shell environment. Adds the application's base directory
//        to the user's PATH variable (if not already present), displays an ASCII splash screen, and
//        prints a welcome message.
//
//  Private Methods:
//      - AddToPath: Adds the specified path to the user's PATH environment variable if not already
//        present, notifies the user of success or failure, and refreshes environment variables.
//      - RefreshEnvironment: Broadcasts a message to refresh environment variables in the current
//        Windows session.
//      - ASCIISplashScreen: Displays an ASCII art splash screen and the ProtonDB logo in the terminal.
//
//  Platform Interop:
//      - SendMessageTimeout: P/Invokes user32.dll to notify all top-level windows of environment
//        variable changes.
//
//  Usage Example:
//      Meta.Loading();
//
//  Dependencies:
//      - Kisetsu.Utils.Terminal: For terminal output and input handling.
//      - System.Runtime.InteropServices: For Windows API interop.
// -------------------------------------------------------------------------------------------------

using Kisetsu.Utils;
using System.Runtime.InteropServices;

namespace ProtonDB.Shell {
    /// <summary>
    /// Provides meta-operations for the ProtonDB shell, such as environment setup and splash screen display.
    /// </summary>
    public static class Meta {
        private const string PROTON_DB = "ProtonDB";

        /// <summary>
        /// Initializes the ProtonDB shell environment.
        /// Adds the application's base directory to the user's PATH variable (if not already present),
        /// displays an ASCII splash screen, and prints a welcome message.
        /// </summary>
        public static void Loading() {
            if (AddToPath(AppContext.BaseDirectory)) {
                ASCIISplashScreen();
                Console.WriteLine($"\nWelcome to {PROTON_DB}!");
            }
        }

        /// <summary>
        /// Adds the specified path to the user's PATH environment variable if it is not already present.
        /// Notifies the user of success or failure and refreshes environment variables.
        /// </summary>
        /// <param name="newPath">The directory path to add to PATH.</param>
        /// <returns>True if the path was added; false if it was already present or an error occurred.</returns>
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

        /// <summary>
        /// Broadcasts a message to refresh environment variables in the current Windows session.
        /// </summary>
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

        /// <summary>
        /// Sends a message to all top-level windows to notify them of environment variable changes.
        /// </summary>
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = false)]
        private static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint Msg, IntPtr wParam, string lParam, uint fuFlags, uint uTimeout, out IntPtr lpdwResult);

        /// <summary>
        /// Displays an ASCII art splash screen and the ProtonDB logo in the terminal.
        /// </summary>
        private static void ASCIISplashScreen() {
            Terminal.WriteLine("\r\n\r\n                                                                      \r\n                               %...=*                                 \r\n                             :........%                               \r\n                           =...........+                              \r\n                          *.............-                             \r\n             #:....+%-   ................*    *#.....#:               \r\n           %............:%...............+@.............              \r\n          *.............#...#:........%.................+             \r\n          %............:.......%...#.......#.............             \r\n          +............#.........*#.....................=             \r\n           ...................*.....%.......%...........%             \r\n           #..........*.+#%%@%+=-:--=*@%%%#-+..........:              \r\n            *..*%-....%...#.............*.........%%....              \r\n           %.:........=.+.................%..=........*.%             \r\n       #...............:....................#%.......#.....*.         \r\n     *.........:.....%........@@@@@@.........%......#.........#       \r\n   :............*..:........@@@@@@@@@#.......#.%...-...........#      \r\n   :.............%%........:@@@@@@@@@@.......#..:-..............*     \r\n   *.............=-.........@@@@@@@@@@.......#..%.*..............     \r\n    -..........:....#.......*@@@@@@@@........#.:...%...........#      \r\n     *........+......-........:@@@@..........@......#........%        \r\n        %....+.........%...................+.#.......#....+-          \r\n            %.........+..%................:..-........@#              \r\n            .....=%%..%....#............+......=%#.....%              \r\n           %..........=......#.:=++=:.+.....#...........              \r\n           ............:.......%...+:.......*...........%             \r\n          #............%.........@:........:............:             \r\n          %...................*:....%......%.............             \r\n           .............%..#:..........%..:.............%             \r\n           :...........*#-................=%...........%              \r\n              +%%%#.      ...............%      *%%%#                 \r\n                           .............%                             \r\n                            ...........%                              \r\n                             %.......+                                \r\n                                %%%=                                  \r");
            Terminal.WriteLine("\r\n\r\n\t______          _               ____________ \r\n\t| ___ \\        | |              |  _  \\ ___ \\\r\n\t| |_/ / __ ___ | |_ ___  _ __   | | | | |_/ /\r\n\t|  __/ '__/ _ \\| __/ _ \\| '_ \\  | | | | ___ \\\r\n\t| |  | | | (_) | || (_) | | | | | |/ /| |_/ /\r\n\t\\_|  |_|  \\___/ \\__\\___/|_| |_| |___/ \\____/ \r\n\t                                             \r\n\t                                             \r\n\r\n");
        }
    }
}