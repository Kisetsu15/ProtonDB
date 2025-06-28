using Kisetsu.Utils;
using Newtonsoft.Json;

namespace ProtonDB.Server {
    namespace Core {
        public class Profiles {

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
                string createdAt = DateTime.UtcNow.ToString("o");
                string salt = AES.GenerateSalt();
                ProfileInfo profile = new(
                    GenerateChecksum(userName, password, privilege, createdAt, salt),
                    salt,
                    privilege,
                    createdAt,
                    privilege.Equals(Privilege.admin) ? [..Meta.GetDatabaseList().Keys] : [Meta.defaultDatabase]
                );

                userConfig.Add(userName, profile);
                Save(Meta.ProfileConfig, userConfig);
                return [$"Profile {privilege} '{userName}' created"];
            }

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


            public static bool UpdateAdminDatabase() {
                var config = Load(Meta.ProfileConfig);
                List<string> databases = [.. Meta.GetDatabaseList().Keys];
                foreach ( var info in config.UserNames) {
                    if (config.TryGetValue(info, out ProfileInfo? profile)) {
                        if (profile != null && profile.Privilege == Privilege.admin) {
                            ProfileInfo temp = new (
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

            private static string[]? SplitArgs(string argument, int required = 1, int max = 2) {
                Terminal.Log($"{argument} {required} {max}");
                var args = argument.Split(',').Select(s => s.Trim().Trim('"')).ToArray();
                if (args.Length < required || args.Length > max) return null;
                foreach (var arg in args) {
                    Terminal.Log(arg);
                }
                return args;
            }

            private static bool ValidateProfile(string userName, string password) {
                var config = Load(Meta.ProfileConfig);
                if (!config.TryGetValue(userName, out var info) || info == null) return false;
                string checksum = GenerateChecksum(userName, password, info.Privilege, info.CreatedAt, info.Salt);
                return checksum.Equals(info.Checksum);
            }

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

            private static string GenerateChecksum(string userName, string password, string privilege, string createdAt, string salt) {
                string key = $"{Storage._protonDB}-{privilege}-{userName}-{password}-{createdAt}-{salt}";
                return Hash.Compute(key, Algorithm.SHA256, Case.Lower);
            }
        }
    }
}
