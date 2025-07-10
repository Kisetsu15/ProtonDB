// -------------------------------------------------------------------------------------------------
//  File: Token.cs
//  Namespace: ProtonDB.Shell
//  Description:
//      Provides a set of constant command tokens used by the ProtonDB shell for command parsing and
//      recognition. This static class centralizes all recognized command keywords and aliases, enabling
//      consistent and maintainable command handling throughout the shell.
//
//  Public Fields:
//      - _help:     Short token for displaying help (":h").
//      - _quit:     Short token for quitting the shell (":q").
//      - quit:      Full command for quitting the shell ("quit").
//      - _version:  Short token for displaying version information (":v").
//      - version:   Long command for displaying version information ("--version").
//      - help:      Long command for displaying help ("--help").
//      - clear:     Command for clearing the shell screen ("cls").
//
//  Usage Example:
//      if (input == Token._help || input == Token.help) { /* Show help */ }
//
//  Dependencies:
//      None.
// -------------------------------------------------------------------------------------------------

namespace ProtonDB.Shell {
    /// <summary>
    /// Provides a set of constant command tokens used by the ProtonDB shell for command parsing and recognition.
    /// </summary>
    public static class Token {
        /// <summary>
        /// Token for displaying help using the short command (<c>:h</c>).
        /// </summary>
        public const string _help = ":h";

        /// <summary>
        /// Token for quitting the shell using the short command (<c>:q</c>).
        /// </summary>
        public const string _quit = ":q";

        /// <summary>
        /// Token for quitting the shell using the full command (<c>quit</c>).
        /// </summary>
        public const string quit = "quit";

        /// <summary>
        /// Token for displaying version information using the short command (<c>:v</c>).
        /// </summary>
        public const string _version = ":v";

        /// <summary>
        /// Token for displaying version information using the long command (<c>--version</c>).
        /// </summary>
        public const string version = "--version";

        /// <summary>
        /// Token for displaying help using the long command (<c>--help</c>).
        /// </summary>
        public const string help = "--help";

        /// <summary>
        /// Token for clearing the shell screen (<c>cls</c>).
        /// </summary>
        public const string clear = "cls";
    }
}