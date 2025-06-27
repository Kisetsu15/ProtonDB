using Kisetsu.Utils;
using Newtonsoft.Json;

namespace ProtonDB.Server {
    namespace Core {
        public class Profiles {

            public static string[] Create(string argument) { 
                if (Meta.CurrentPrivilege != Privilege.admin) {                     
                    return ["Requires admin privilege"];
                }
                var arg = Parse(argument, 2, 3);
                if (arg == null) {
                     return ["Invalid Argument"];
                }

                string? userName = arg.Value.arg1;
                string? password = arg.Value.arg2;
                string? privilege = arg.Value.arg3;

                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(privilege)) {
                    return ["Values cannot be empty"];
                }

                var userConfig = Load(Token._profileConfig);
                string createdAt = DateTime.UtcNow.ToString("o");
                string salt = AES.GenerateSalt();
                ProfileInfo profile = new(
                    GenerateChecksum(userName, password, privilege, createdAt, salt),
                    salt,
                    privilege,
                    createdAt,
                    privilege == Privilege.admin ?[..Meta.GetDatabaseList().Keys] :[Meta.defaultDatabase]
                );

                userConfig.Add(userName, profile);
                Save(Token._profileConfig, userConfig);
                return [$"Profile {privilege} '{userName}' created"];
            }

            private static (string? arg1, string? arg2, string? arg3)? Parse(string Argument, int min = 1, int max = 2) {
                string[] args = Argument.Split(',');
                int length = args.Length;
                if (length < min || length > max) return null;
                return (args[0].Trim().Trim('"'),
                    args[1].Trim().Trim('"'),
                    length > 2 ? args[2].Trim().Trim('"') : Privilege.user);
            }


            public static string[] Delete(string userName) {

                if (Meta.CurrentPrivilege != Privilege.admin) {
                    return ["Requires admin privilege"];
                }
                if (string.IsNullOrWhiteSpace(userName)) {
                    return ["Username cannot be empty"];
                }
                if (userName.Equals(Meta.CurrentUser, StringComparison.OrdinalIgnoreCase)) {
                    return ["Cannot delete the current profile"];
                }
                var userConfig = Load(Token._profileConfig);
                if (userConfig.TryGetValue(userName, out ProfileInfo? profile)) {
                    if (profile == null) {
                        userConfig.Remove(userName);
                        Save(Token._profileConfig, userConfig);
                        return ["Profile not found"];
                    }
                    userConfig.Remove(userName);
                    Save(Token._profileConfig, userConfig);
                    return [$"Profile '{userName}' deleted"];
                } else {
                    return [$"Profile '{userName}' does not exist"];
                }
            }


            public static string[] Login(string argument) {

                var arg =  Parse(argument);
                if (arg == null) {
                    return ["Invalid Argument"];
                }
                string? userName = arg.Value.arg1;
                string? password = arg.Value.arg2;

                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password)) {
                    return ["Username and password cannot be empty"];
                }
                if (Meta.CurrentUser != userName) {
                    return ["Already logged in"];
                }

                if (ValidateProfile(userName, password)) {
                    ProfileInfo? profileInfo = Load(Token._profileConfig)[userName];
                    if (profileInfo == null) {
                        return ["Profile not found"];
                    }

                    Meta.CurrentProfile = new Profile {
                        profileName = userName,
                        profileInfo = profileInfo
                    };
                    return [$"Login successful"];
                } else {
                    return ["Invalid credentials"];
                }
            }

            public static string[] List() {
                if (Meta.CurrentPrivilege != Privilege.admin) {
                    return ["Requires admin privilege"];
                }
                var userConfig = Load(Token._profileConfig);
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

            public static string[] Grant(string Argument) {
                var arg = Parse(Argument);
                if (arg == null) {
                    return ["Invalid Argument"];
                }
                string? userName = arg.Value.arg1;
                string? database = arg.Value.arg2;

                if (Meta.CurrentPrivilege == Privilege.guest) {
                    return ["Guest cannot grant access"];
                }
                if (string.IsNullOrWhiteSpace(userName)) {
                    return ["Username cannot be empty"];
                }
                if (string.IsNullOrWhiteSpace(database)) {
                    database = Meta.CurrentDatabase;
                }
                return GrantAccess(userName, database);
            }

            public static string[] Revoke(string Argument) {
                var arg = Parse(Argument);
                if (arg == null) {
                    return ["Invalid Argument"];
                }
                string? userName = arg.Value.arg1;
                string? database = arg.Value.arg2;
                if (Meta.CurrentPrivilege == Privilege.guest) {
                    return ["Guest cannot revoke access"];
                }
                if (string.IsNullOrWhiteSpace(userName)) {
                    return ["Username cannot be empty"];
                }
                if (string.IsNullOrWhiteSpace(database)) {
                    database = Meta.CurrentDatabase;
                }
                return RevokeAccess(userName, database);
            }


            public static Profile Guest() {
                const string password = "welcome";
                string guestName = AES.GenerateSalt();
                string salt = AES.GenerateSalt();
                string createdAt = DateTime.UtcNow.ToString("o");
                ProfileInfo guestInfo = new(
                    Checksum: GenerateChecksum(guestName, password, Privilege.guest, createdAt, salt),
                    Salt: salt,
                    Privilege: Privilege.guest,
                    CreatedAt: createdAt,
                    Database: [Meta.defaultDatabase]
                );

                HashMap userConfig = Load(Meta.UserConfigFile);
                userConfig.Add(guestName, guestInfo);
                Save(Meta.UserConfigFile, userConfig);

                return new Profile {
                    profileName = guestName,
                    profileInfo = guestInfo
                };
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

                HashMap userConfig = Load(Meta.UserConfigFile);
                userConfig.Add(adminName, adminInfo);
                Save(Meta.UserConfigFile, userConfig);

                return new Profile {
                    profileName = adminName,
                    profileInfo = adminInfo
                };
            }

            public static bool ValidateAccess(string database) {
                if (Meta.CurrentPrivilege == Privilege.admin) {
                    return true;
                }

                var userInfo = Load(Token._profileConfig);
                if (userInfo.TryGetValue(Meta.CurrentUser, out var info)) {
                    if (info == null) {
                        userInfo.Remove(Meta.CurrentUser);
                        Meta.CurrentProfile = Guest();
                        Save(Token._profileConfig, userInfo);
                        return false;
                    }
                    return info.Database.Contains(database);
                } 
                return false;
            }

            public static string[] GrantAccess(string userName, string database) {
                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(database)) {
                    return ["Username or database cannot be empty"];
                }
                var userConfig = Load(Token._profileConfig);
                if (userConfig.TryGetValue(userName, out ProfileInfo? profile)) {
                    if (Meta.CurrentPrivilege == Privilege.admin || Meta.CurrentUserDatabases.Contains(database)) {
                        if (profile == null) {
                            return ["Profile not found"];
                        }
                        if (!profile.Database.Contains(database)) {
                            profile.Database.Add(database);
                            Save(Token._profileConfig, userConfig);
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

            public static string[] RevokeAccess(string userName, string database) {
                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(database)) {
                    return ["Username or database cannot be empty"];
                }
                var userConfig = Load(Token._profileConfig);
                if (userConfig.TryGetValue(userName, out ProfileInfo? profile)) {
                    if (profile == null) {
                        return ["Profile not found"];
                    }
                    if (profile.Database.Contains(database)) {
                        profile.Database.Remove(database);
                        Save(Token._profileConfig, userConfig);
                        return [$"Access revoked from {database} for {userName}"];
                    } else {
                        return [$"{userName} does not have access to {database}"];
                    }
                } else {
                    return ["Profile not found"];
                }
            }

            public static bool UpdateAdminDatabase() {
                var config = Load(Token._profileConfig);
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
                            return true;
                        }
                    }
                }
                return false;
            }

            private static bool ValidateProfile(string userName, string password) {
                var profiles = Load(Token._profileConfig);
                if (profiles.TryGetValue(userName, out var info)) {
                    if (info == null) {
                        return false;
                    }
                    string checksum = GenerateChecksum(userName, password, info.Privilege, info.CreatedAt, info.Salt);
                    return checksum.Equals(info.Checksum);
                }
                return false;
            }

            private static HashMap Load(string filePath) {
                if (!File.Exists(filePath)) {
                    return new();
                }

                var dict = JsonConvert.DeserializeObject<Dictionary<string, ProfileInfo>>(File.ReadAllText(filePath));
                if (dict == null) {
                    return new();
                }
                HashMap hashMap = new();
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
                File.WriteAllText(filePath, JsonConvert.SerializeObject(dict, Formatting.Indented));
                return true;
            }

            private static string GenerateChecksum(string userName, string password, string privilege, string createdAt, string salt) {
                string key = $"ProtonDB-{privilege}-{userName}-{password}-{createdAt}-{salt}";
                return Hash.Compute(key, Algorithm.SHA256, Case.Lower);
            }
        }
    }
}
