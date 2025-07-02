// -------------------------------------------------------------------------------------------------
//  File: HashMap.cs
//  Namespace: ProtonDB.Server.Core
//  Description:
//      Defines user profile data structures and a HashMap class for managing user profiles
//      in memory. The HashMap provides efficient add, update, remove, and lookup operations
//      for user profile information, supporting authentication and authorization features
//      in the ProtonDB server.
//
//  Types:
//      - Profile: Struct representing a user profile with a name and associated profile info.
//      - ProfileInfo: Record containing user authentication and privilege metadata.
//      - HashMap: Class encapsulating a dictionary of user profiles with methods for
//        management and retrieval.
//
//  Usage Example:
//      var map = new HashMap();
//      map.Add("alice", new ProfileInfo(...));
//      if (map.TryGetValue("alice", out var info)) { /* use info */ }
//
//  Dependencies:
//      - System.Collections.Generic
// -------------------------------------------------------------------------------------------------

namespace ProtonDB.Server {
    namespace Core {
        /// <summary>
        /// Represents a user profile with a name and associated profile information.
        /// </summary>
        public struct Profile {
            public string profileName;
            public ProfileInfo profileInfo;
        }

        /// <summary>
        /// Contains user authentication and privilege metadata.
        /// </summary>
        /// <param name="Checksum">Password or credential checksum.</param>
        /// <param name="Salt">Salt used for hashing credentials.</param>
        /// <param name="Privilege">Privilege level or role of the user.</param>
        /// <param name="CreatedAt">Timestamp of profile creation.</param>
        /// <param name="Database">List of databases accessible to the user.</param>
        public record ProfileInfo(
            string Checksum,
            string Salt,
            string Privilege,
            string CreatedAt,
            List<string> Database
        );

        /// <summary>
        /// Provides an in-memory mapping of user names to their profile information.
        /// Supports add, update, remove, and lookup operations.
        /// </summary>
        public class HashMap {
            private readonly Dictionary<string, ProfileInfo> _users = [];

            /// <summary>
            /// Gets all user names in the map.
            /// </summary>
            public IEnumerable<string> UserNames => _users.Keys;

            /// <summary>
            /// Adds a new user profile if the user name does not already exist.
            /// </summary>
            /// <param name="userName">The user name.</param>
            /// <param name="info">The profile information.</param>
            /// <returns>True if added; false if the user already exists.</returns>
            public bool Add(string userName, ProfileInfo info) {
                return _users.TryAdd(userName, info);
            }

            /// <summary>
            /// Sets or updates the profile information for a user.
            /// </summary>
            /// <param name="userName">The user name.</param>
            /// <param name="info">The profile information.</param>
            public void Set(string userName, ProfileInfo info) {
                _users[userName] = info;
            }

            /// <summary>
            /// Removes a user profile by user name.
            /// </summary>
            /// <param name="userName">The user name.</param>
            /// <returns>True if the user was removed; false if not found.</returns>
            public bool Remove(string userName) {
                return _users.Remove(userName);
            }

            /// <summary>
            /// Attempts to get the profile information for a user.
            /// </summary>
            /// <param name="userName">The user name.</param>
            /// <param name="profile">The profile information if found; otherwise, null.</param>
            /// <returns>True if found; otherwise, false.</returns>
            public bool TryGetValue(string userName, out ProfileInfo? profile) {
                return _users.TryGetValue(userName, out profile);
            }

            /// <summary>
            /// Gets the profile information for a user, or null if not found.
            /// </summary>
            /// <param name="userName">The user name.</param>
            public ProfileInfo? this[string userName] =>
                _users.TryGetValue(userName, out var profile) ? profile : null;
        }
    }
}s