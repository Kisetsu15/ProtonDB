// -------------------------------------------------------------------------------------------------
//  File: QuerySession.cs
//  Namespace: ProtonDB.Server
//  Description:
//      Represents the state and context of a client session in the ProtonDB server. Tracks
//      authentication status, user profile, last query, query results, debug mode, and the
//      current database in use. Provides convenient accessors for the current user's name,
//      privilege, and accessible databases, as well as session control flags.
//
//  Public Properties:
//      - LastQuery: The last query string received from the client.
//      - Result: The result of the last executed query, as an array of strings.
//      - Debug: Indicates whether debug mode is enabled for this session.
//      - ShouldExit: Signals if the session should be terminated by the server.
//      - IsAuthenticated: Indicates if the user has successfully authenticated.
//      - CurrentProfile: The user's profile, including name and privilege information.
//      - CurrentUser: The current user's name (from the profile).
//      - CurrentPrivilege: The current user's privilege level (from the profile).
//      - CurrentUserDatabases: The list of databases the user has access to (from the profile).
//      - CurrentDatabase: The name of the database currently in use for this session.
//
//  Usage Example:
//      var session = new QuerySession();
//      session.IsAuthenticated = true;
//      session.CurrentProfile = ...;
//      var user = session.CurrentUser;
//      var db = session.CurrentDatabase;
//
//  Dependencies:
//      - Profile: Represents a user profile with name and privilege info.
//      - Meta: Provides the default database name.
// -------------------------------------------------------------------------------------------------

using ProtonDB.Server.Core;

namespace ProtonDB.Server {
    /// <summary>
    /// Represents the state and context of a client session in the ProtonDB server.
    /// Tracks authentication, user profile, last query, results, debug mode, and current database.
    /// </summary>
    public class QuerySession {
        /// <summary>
        /// The last query string received from the client.
        /// </summary>
        public string? LastQuery { get; set; }

        /// <summary>
        /// The result of the last executed query, as an array of strings.
        /// </summary>
        public string[]? Result { get; set; }

        /// <summary>
        /// Indicates whether debug mode is enabled for this session.
        /// </summary>
        public bool Debug { get; set; } = false;

        /// <summary>
        /// Signals if the session should be terminated by the server.
        /// </summary>
        public bool ShouldExit { get; set; } = false;

        /// <summary>
        /// Indicates if the user has successfully authenticated.
        /// </summary>
        public bool IsAuthenticated { get; set; } = false;

        /// <summary>
        /// The user's profile, including name and privilege information.
        /// </summary>
        public Profile CurrentProfile { get; set; } = new();

        /// <summary>
        /// The current user's name (from the profile).
        /// </summary>
        public string CurrentUser => CurrentProfile.profileName;

        /// <summary>
        /// The current user's privilege level (from the profile).
        /// </summary>
        public string CurrentPrivilege => CurrentProfile.profileInfo.Privilege;

        /// <summary>
        /// The list of databases the user has access to (from the profile).
        /// </summary>
        public List<string> CurrentUserDatabases => CurrentProfile.profileInfo.Database;

        /// <summary>
        /// The name of the database currently in use for this session.
        /// </summary>
        public string CurrentDatabase { get; set; } = Meta.defaultDatabase;
    }
}