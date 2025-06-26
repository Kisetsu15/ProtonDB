using Kisetsu.Utils;
using Newtonsoft.Json;

namespace ProtonDB.Server {
    namespace Core {
        public class Profiles {

            public static string[] Create(string Argument) { 

                string[] args = Argument.Split(',');
                if (args.Length < 2) {
                     return ["Invalid Argument"];
                }
                string userName = args[0].Trim().Trim('"');
                string password = args[1].Trim().Trim('"');
                string privilege = args.Length > 2 ? args[2].Trim().Trim('"') : Token.user;

                if (Meta.CurrentPrivilege != Token.admin) {                     
                    return ["Requires admin privilege"];
                }   

                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password)) {
                    return ["Username and password cannot be empty"];
                }

                var userConfig = Load(Token._userConfigFile);
                string createdAt = DateTime.UtcNow.ToString("o");
                ProfileInfo profile = new(
                    GenerateChecksum(userName, password, privilege, createdAt),
                    privilege,
                    createdAt,
                    privilege == Token.admin ?[..Meta.GetDatabaseList().Keys] :[Meta.defaultDatabase]
                );

                userConfig.Add(userName, profile);
                Save(Token._userConfigFile, userConfig);
                return [$"Profile {privilege} '{userName}' created"];
            }


            public static string[] Delete(string userName) {

                if (Meta.CurrentPrivilege != Token.admin) {
                    return ["Requires admin privilege"];
                }
                if (string.IsNullOrWhiteSpace(userName)) {
                    return ["Username cannot be empty"];
                }
                if (userName.Equals(Meta.CurrentUser, StringComparison.OrdinalIgnoreCase)) {
                    return ["Cannot delete the current profile"];
                }
                var userConfig = Load(Token._userConfigFile);
                if (userConfig.TryGetValue(userName, out ProfileInfo? profile)) {
                    if (profile == null) {
                        userConfig.Remove(userName);
                        Save(Token._userConfigFile, userConfig);
                        return ["Profile not found"];
                    }
                    userConfig.Remove(userName);
                    Save(Token._userConfigFile, userConfig);
                    return [$"Profile '{userName}' deleted"];
                } else {
                    return [$"Profile '{userName}' does not exist"];
                }
            }


            public static string[] Login(string Argument) {

                string[] args = Argument.Split(',');
                if (args.Length < 2) {
                    return ["Invalid Argument"];
                }
                string userName = args[0].Trim().Trim('"');
                string password = args[1].Trim().Trim('"');

                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password)) {
                    return ["Username and password cannot be empty"];
                }
                if (Meta.CurrentUser != userName) {
                    return ["Already logged in"];
                }

                if (ValidateProfile(userName, password)) {
                    ProfileInfo? profileInfo = Load(Token._userConfigFile)[userName];
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
                if (Meta.CurrentPrivilege != Token.admin) {
                    return ["Requires admin privilege"];
                }
                var userConfig = Load(Token._userConfigFile);
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


            public static Profile Guest() {
                string guestName = AES.GenerateSalt();
                string createdAt = DateTime.UtcNow.ToString("o");

                ProfileInfo guestInfo = new(
                    Checksum: GenerateChecksum(guestName, "welcome", Token.guest, createdAt),
                    Privilege: Token.guest,
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
                ProfileInfo adminInfo = new(
                    Checksum: GenerateChecksum(adminName, password, Token.admin, createdAt),
                    Privilege: Token.admin,
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
                if (Meta.CurrentPrivilege == Token.admin) {
                    return true;
                }

                var userInfo = Load(Token._userConfigFile);
                if (userInfo.TryGetValue(Meta.CurrentUser, out var info)) {
                    if (info == null) {
                        userInfo.Remove(Meta.CurrentUser);
                        Meta.CurrentProfile = Guest();
                        Save(Token._userConfigFile, userInfo);
                        return false;
                    }
                    var databases = info.Database;
                    return databases.Contains(database);
                } 
                return false;
            }

            private static bool ValidateProfile(string userName, string password) {
                var userInfo = Load(Token._userConfigFile);
                if (userInfo.TryGetValue(userName, out var info)) {
                    if (info == null) {
                        return false;
                    }
                    string checksum = GenerateChecksum(userName, password, info.Privilege, info.CreatedAt);
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

            private static string GenerateChecksum(string userName, string password, string privilege, string createdAt) {
                string key = $"ProtonDB-{privilege}-{userName}-{password}-{createdAt}";
                return Hash.Compute(key, Algorithm.SHA256, Case.Lower);
            }
        }
    }
}
