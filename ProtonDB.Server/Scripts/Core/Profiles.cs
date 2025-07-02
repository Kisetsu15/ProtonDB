// -------------------------------------------------------------------------------------------------
//  File: Profiles.cs
//  Namespace: ProtonDB.Server.Core
//  Description:
//      Provides static methods for managing user profiles, authentication, and access control
//      in ProtonDB. Supports creation, deletion, login, privilege management, and database
//      access grants/revokes for user accounts. Profiles are persisted in a configuration file
//      and use salted checksums for credential security.
//
//  Public Methods:
//      - Create: Creates a new user profile with specified privilege and credentials.
//      - Delete: Deletes an existing user profile (cannot delete self).
//      - Login: Authenticates a user and updates the session with profile info.
//      - List: Lists all user profiles (admin only).
//      - Grant: Grants database access to a user.
//      - Revoke: Revokes database access from a user.
//      - Admin: Creates or updates the admin profile with full database access.
//      - ValidateAccess: Checks if the session user has access to a database.
//      - UpdateAdminDatabase: Updates all admin profiles with the current database list.
//      - UpdateDatabase: Adds or removes a database from the current user's accessible list.
//
//  Internal Methods:
//      - GrantAccess: Helper for granting database access to a user.
//      - RevokeAccess: Helper for revoking database access from a user.
//      - SplitArgs: Utility for parsing argument strings.
//      - ValidateProfile: Validates user credentials against stored checksum.
//      - Load: Loads the profile configuration from disk into a HashMap.
//      - Save: Persists the HashMap to disk as JSON.
//      - GenerateChecksum: Generates a salted hash for user credentials.
//
//  Dependencies:
//      - QuerySession: Represents the current user session and context.
//      - Profile, ProfileInfo, HashMap: User profile data structures.
//      - Privilege: String constants for privilege levels (e.g., admin, user).
//      - Meta: Provides configuration paths and database metadata.
//      - AES, Hash: Utility classes for cryptography and hashing.
//      - Kisetsu.Utils, Newtonsoft.Json: Utility and serialization libraries.
//
//  Usage Example:
//      var result = Profiles.Create("alice,pass123,admin", session);
//      var loginSuccess = Profiles.Login("alice", "pass123", session);
//      var list = Profiles.List(session);
// -------------------------------------------------------------------------------------------------

using Kisetsu.Utils;
using Newtonsoft.Json;

namespace ProtonDB.Server {
    namespace Core {
        /// <summary>
        /// Provides static methods for managing user profiles, authentication, and access control.
        /// </summary>
        public class Profiles {

            /// <summary>
            /// Creates a new user profile with the specified credentials and privilege.
            /// Only users with admin privilege can create profiles.
            /// </summary>
            /// <param name="argument">Comma-separated: username, password, [privilege]</param>
            /// <param name="session">The current query session.</param>
            /// <returns>Status or error messages.</returns>
            public static string[] Create(string argument, QuerySession session) {
                if (session.CurrentPrivilege != Privilege.admin) {
                    return ["Requires admin privilege"];
                }

                var arg = SplitArgs(argument, 2, 3);
                if (arg == null) {
                    return ["Invalid Argument"];
                }

                string userName = arg[0];
                string password = arg[1];
                string privilege = (arg.Length == 3) ? arg[2] : Privilege.user;

                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password)) {
                    return ["Values cannot be empty"];
                }

                var userConfig = Load(Meta.ProfileConfig);
                if (userConfig.TryGetValue(userName, out _)) {
                    return ["Profile already exists"];
                }

                string createdAt = DateTime.UtcNow.ToString("o");
                string salt = AES.GenerateSalt();
                ProfileInfo profile = new(
                    GenerateChecksum(userName, password, privilege, createdAt, salt),
                    salt,
                    privilege,
                    createdAt,
                    privilege.Equals(Privilege.admin) ? [.. Meta.GetDatabaseList().Keys] : [Meta.defaultDatabase]
                );

                userConfig.Add(userName, profile);
                Save(Meta.ProfileConfig, userConfig);
                return [$"Profile {privilege} '{userName}' created"];
            }

            /// <summary>
            /// Deletes an existing user profile. Cannot delete the current user's own profile.
            /// Only users with admin privilege can delete profiles.
            /// </summary>
            /// <param name="userName">The username to delete.</param>
            /// <param name="session">The current query session.</param>
            /// <returns>Status or error messages.</returns>
            public static string[] Delete(string userName, QuerySession session) {

                if (session.CurrentPrivilege != Privilege.admin) {
                    return ["Requires admin privilege"];
                }
                if (string.IsNullOrWhiteSpace(userName)) {
                    return ["Username cannot be empty"];
                }
                if (userName.Equals(session.CurrentUser, StringComparison.OrdinalIgnoreCase)) {
                    return ["Cannot delete the current profile"];
                }
                var userConfig = Load(Meta.ProfileConfig);
                if (userConfig.TryGetValue(userName, out ProfileInfo? profile)) {
                    if (profile == null) {
                        userConfig.Remove(userName);
                        Save(Meta.ProfileConfig, userConfig);
                        return ["Profile not found"];
                    }
                    userConfig.Remove(userName);
                    Save(Meta.ProfileConfig, userConfig);
                    return [$"Profile '{userName}' deleted"];
                } else {
                    return [$"Profile '{userName}' does not exist"];
                }
            }

            /// <summary>
            /// Authenticates a user and updates the session with profile information if successful.
            /// </summary>
            /// <param name="userName">The username.</param>
            /// <param name="password">The password.</param>
            /// <param name="session">The current query session.</param>
            /// <returns>True if login is successful; otherwise, false.</returns>
            public static bool Login(string userName, string password, QuerySession session) {
                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password)) return false;
                if (ValidateProfile(userName, password)) {
                    ProfileInfo? profileInfo = Load(Meta.ProfileConfig)[userName];
                    if (profileInfo == null) return false;
                    session.CurrentProfile = new Profile {
                        profileName = userName,
                        profileInfo = profileInfo
                    };

                    return true;
                }
                return false;
            }

            /// <summary>
            /// Lists all user profiles with privilege and creation date. Admin only.
            /// </summary>
            /// <param name="session">The current query session.</param>
            /// <returns>List of profiles or error message.</returns>
            public static string[] List(QuerySession session) {
                if (session.CurrentPrivilege != Privilege.admin) {
                    return ["Requires admin privilege"];
                }
                var userConfig = Load(Meta.ProfileConfig);
                if (userConfig == null) {
                    return ["No profiles found"];
                }

                List<string> profiles = [];
                foreach (var userName in userConfig.UserNames) {
                    if (userConfig.TryGetValue(userName, out ProfileInfo? profile)) {
                        if (profile != null) {
                            profiles.Add($"{userName}\t{profile.Privilege}\t{profile.CreatedAt}");
                        }
                    }
                }
                return [.. profiles];
            }

            /// <summary>
            /// Grants database access to a user.
            /// </summary>
            /// <param name="Argument">Comma-separated: username, database</param>
            /// <param name="session">The current query session.</param>
            /// <returns>Status or error messages.</returns>
            public static string[] Grant(string Argument, QuerySession session) {
                var arg = SplitArgs(Argument);
                if (arg == null) {
                    return ["Invalid Argument"];
                }
                string? userName = arg[0];
                string? database = arg[1];

                if (string.IsNullOrWhiteSpace(userName)) {
                    return ["Username cannot be empty"];
                }
                if (string.IsNullOrWhiteSpace(database)) {
                    database = session.CurrentDatabase;
                }
                return GrantAccess(userName, database, session);
            }

            /// <summary>
            /// Revokes database access from a user.
            /// </summary>
            /// <param name="Argument">Comma-separated: username, database</param>
            /// <param name="session">The current query session.</param>
            /// <returns>Status or error messages.</returns>
            public static string[] Revoke(string Argument, QuerySession session) {
                var arg = SplitArgs(Argument);
                if (arg == null) {
                    return ["Invalid Argument"];
                }
                string? userName = arg[0];
                string? database = arg[1];
                if (string.IsNullOrWhiteSpace(userName)) {
                    return ["Username cannot be empty"];
                }
                if (string.IsNullOrWhiteSpace(database)) {
                    database = session.CurrentDatabase;
                }
                return RevokeAccess(userName, database, session);
            }

            /// <summary>
            /// Creates or updates the admin profile with full access to all databases.
            /// </summary>
            /// <param name="adminName">The admin username.</param>
            /// <param name="password">The admin password.</param>
            /// <returns>The created admin profile.</returns>
            public static Profile Admin(string adminName, string password) {
                string createdAt = DateTime.UtcNow.ToString("o");
                string salt = AES.GenerateSalt();
                ProfileInfo adminInfo = new(
                    Checksum: GenerateChecksum(adminName, password, Privilege.admin, createdAt, salt),
                    Salt: salt,
                    Privilege: Privilege.admin,
                    CreatedAt: createdAt,
                    Database: [.. Meta.GetDatabaseList().Keys]
                );

                HashMap userConfig = Load(Meta.ProfileConfig);
                userConfig.Add(adminName, adminInfo);
                Save(Meta.ProfileConfig, userConfig);

                return new Profile {
                    profileName = adminName,
                    profileInfo = adminInfo
                };
            }

            /// <summary>
            /// Checks if the session user has access to the specified database.
            /// Admins always have access.
            /// </summary>
            /// <param name="database">The database name.</param>
            /// <param name="session">The current query session.</param>
            /// <returns>True if access is allowed; otherwise, false.</returns>
            public static bool ValidateAccess(string database, QuerySession session) {
                if (session.CurrentPrivilege == Privilege.admin) {
                    return true;
                }

                var userInfo = Load(Meta.ProfileConfig);
                if (userInfo.TryGetValue(session.CurrentUser, out var info)) {
                    if (info == null) {
                        userInfo.Remove(session.CurrentUser);
                        session.CurrentProfile = new();
                        Save(Meta.ProfileConfig, userInfo);
                        return false;
                    }
                    return info.Database.Contains(database);
                }
                return false;
            }

            /// <summary>
            /// Updates all admin profiles with the current list of databases.
            /// </summary>
            /// <returns>True if an admin profile was updated; otherwise, false.</returns>
            public static bool UpdateAdminDatabase() {
                var config = Load(Meta.ProfileConfig);
                List<string> databases = [.. Meta.GetDatabaseList().Keys];
                foreach (var info in config.UserNames) {
                    if (config.TryGetValue(info, out ProfileInfo? profile)) {
                        if (profile != null && profile.Privilege == Privilege.admin) {
                            ProfileInfo temp = new(
                                profile.Checksum,
                                profile.Salt,
                                profile.Privilege,
                                profile.CreatedAt,
                                databases
                            );
                            config.Set(info, temp);
                            Save(Meta.ProfileConfig, config);
                            return true;
                        }
                    }
                }
                return false;
            }

            /// <summary>
            /// Adds or removes a database from the current user's accessible list.
            /// Only admins can update their database list.
            /// </summary>
            /// <param name="databaseName">The database name.</param>
            /// <param name="action">The action to perform (add or drop).</param>
            /// <param name="session">The current query session.</param>
            /// <returns>True if the update was successful; otherwise, false.</returns>
            public static bool UpdateDatabase(string databaseName, Action action, QuerySession session) {
                var config = Load(Meta.ProfileConfig);
                if (config.TryGetValue(session.CurrentUser, out ProfileInfo? profile)) {
                    if (profile != null && profile.Privilege == Privilege.admin) {
                        if (action == Action.add && !profile.Database.Contains(databaseName)) {
                            profile.Database.Add(databaseName);
                        } else if (action == Action.drop && profile.Database.Contains(databaseName)) {
                            profile.Database.Remove(databaseName);
                        } else {
                            return false;
                        }
                        profile.Database.Add(databaseName);
                        return Save(Meta.ProfileConfig, config);
                    }
                }
                return false;
            }

            /// <summary>
            /// Helper for granting database access to a user.
            /// </summary>
            private static string[] GrantAccess(string userName, string database, QuerySession session) {
                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(database)) {
                    return ["Username or database cannot be empty"];
                }
                var userConfig = Load(Meta.ProfileConfig);
                if (userConfig.TryGetValue(userName, out ProfileInfo? profile) && profile != null) {
                    if (session.CurrentPrivilege == Privilege.admin || session.CurrentUserDatabases.Contains(database)) {
                        if (!profile.Database.Contains(database)) {
                            profile.Database.Add(database);
                            Save(Meta.ProfileConfig, userConfig);
                            return [$"Access granted to {database} for {userName}"];
                        } else {
                            return [$"{userName} already has access to {database}"];
                        }
                    } else {
                        return ["You do not have permission to grant access"];
                    }
                } else {
                    return ["Profile not found"];
                }
            }

            /// <summary>
            /// Helper for revoking database access from a user.
            /// </summary>
            private static string[] RevokeAccess(string userName, string database, QuerySession session) {
                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(database)) {
                    return ["Username or database cannot be empty"];
                }

                var userConfig = Load(Meta.ProfileConfig);
                if (!userConfig.TryGetValue(userName, out var profile) || profile == null)
                    return ["Profile not found"];
                if (session.CurrentPrivilege == Privilege.admin || session.CurrentUserDatabases.Contains(database)) {
                    if (profile.Database.Remove(database)) {
                        Save(Meta.ProfileConfig, userConfig);
                        return [$"Access revoked from {database} for {userName}"];
                    } else {
                        return [$"{userName} does not have access to {database}"];
                    }
                } else {
                    return ["You do not have permission to revoke access"];
                }
            }

            /// <summary>
            /// Utility for parsing argument strings into an array.
            /// </summary>
            private static string[]? SplitArgs(string argument, int required = 1, int max = 2) {
                var args = argument.Split(',').Select(s => s.Trim().Trim('"')).ToArray();
                if (args.Length < required || args.Length > max) return null;
                return args;
            }

            /// <summary>
            /// Validates user credentials against the stored checksum.
            /// </summary>
            private static bool ValidateProfile(string userName, string password) {
                var config = Load(Meta.ProfileConfig);
                if (!config.TryGetValue(userName, out var info) || info == null) return false;
                string checksum = GenerateChecksum(userName, password, info.Privilege, info.CreatedAt, info.Salt);
                return checksum.Equals(info.Checksum);
            }

            /// <summary>
            /// Loads the profile configuration from disk into a HashMap.
            /// </summary>
            private static HashMap Load(string filePath) {
                HashMap hashMap = new();
                if (!File.Exists(filePath)) {
                    if (!File.Exists(filePath)) {
                        File.WriteAllText(filePath, "{}");
                        return new();
                    }
                }

                var dict = JsonConvert.DeserializeObject<Dictionary<string, ProfileInfo>>(File.ReadAllText(filePath));
                if (dict == null) {
                    return new();
                }
                foreach (var kvp in dict) {
                    if (kvp.Value != null) {
                        hashMap.Add(kvp.Key, kvp.Value);
                    }
                }
                return hashMap;
            }

            /// <summary>
            /// Persists the HashMap to disk as JSON.
            /// </summary>
            private static bool Save(string filePath, HashMap hashMap) {
                var dict = new Dictionary<string, ProfileInfo>();
                foreach (var userName in hashMap.UserNames) {
                    if (hashMap.TryGetValue(userName, out ProfileInfo? profile)) {
                        if (profile != null) {
                            dict[userName] = profile;
                        } else {
                            hashMap.Remove(userName);
                        }
                    }
                }
                try {
                    File.WriteAllText(filePath, JsonConvert.SerializeObject(dict, Formatting.Indented));
                    return true;
                } catch { return false; }
            }

            /// <summary>
            /// Generates a salted hash for user credentials.
            /// </summary>
            private static string GenerateChecksum(string userName, string password, string privilege, string createdAt, string salt) {
                string key = $"{Storage._protonDB}-{privilege}-{userName}-{password}-{createdAt}-{salt}";
                return Hash.Compute(key, Algorithm.SHA256, Case.Lower);
            }
        }
    }
}