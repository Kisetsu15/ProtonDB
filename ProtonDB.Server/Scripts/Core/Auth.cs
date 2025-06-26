using Kisetsu.Utils;
using Newtonsoft.Json;

namespace ProtonDB.Server {
    namespace Core {
        public class Auth {
            public readonly static string userfile = Path.Combine(Meta.CoreDirectory,"UserConfig.json");
            

            public static string[] Create(string userName, string password, string privilege) { 

                if (Meta.CurrentPrivilege != Token.admin) {                     
                    return ["Requires admin privilege"];
                }   

                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password)) {
                    return ["Username and password cannot be empty"];
                }

                var userConfig = Load(userfile);
                string createdAt = DateTime.UtcNow.ToString("o");
                ProfileInfo profile = new(
                    GenerateChecksum(userName, password, privilege, createdAt),
                    privilege,
                    createdAt,
                    privilege == Token.admin ?[..Meta.GetDatabaseList().Keys] :[Meta.defaultDatabase]
                );

                userConfig.Add(userName, profile);
                Save(userfile, userConfig);
                return [$"Profile {privilege} '{userName}' created"];
            }


            public static string[] Delete(string userName, string currentPrivilege) {
                if (currentPrivilege != Token.admin) {
                    return ["Requires admin privilege"];
                }
                if (string.IsNullOrWhiteSpace(userName)) {
                    return ["Username cannot be empty"];
                }
                if (userName.Equals(Meta.CurrentUser, StringComparison.OrdinalIgnoreCase)) {
                    return ["Cannot delete the current profile"];
                }
                var userConfig = Load(userfile);
                if (userConfig.TryGetValue(userName, out ProfileInfo? profile)) {
                    if (profile == null) {
                        userConfig.Remove(userName);
                        Save(userfile, userConfig);
                        return ["Profile not found"];
                    }
                    userConfig.Remove(userName);
                    Save(userfile, userConfig);
                    return [$"Profile '{userName}' deleted"];
                } else {
                    return [$"Profile '{userName}' does not exist"];
                }
            }

            private static string GenerateChecksum(string userName, string password, string privilege, string createdAt) {
                string key = $"ProtonDB-{privilege}-{userName}-{password}-{createdAt}";
                return Hash.Compute(key, Algorithm.SHA256, Case.Lower);
            }

            public static string[] Login(string userName, string password) {
                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password)) {
                    return ["Username and password cannot be empty"];
                }
                if (ValidateProfile(userName, password, out string privilege)) {
                    Meta.CurrentUser = userName;
                    Meta.CurrentPrivilege = privilege;
                    return [$"Login successful"];
                } else {
                    return ["Invalid username or password"];
                }
            }

            private static bool ValidateProfile(string userName, string password, out string privilege) {
                var userInfo = Load(userfile);
                if (userInfo.TryGetValue(userName, out var info)) {
                    if (info == null) {
                        privilege = Token.guest;
                        return false;
                    }
                    privilege = info.Privilege;
                    string checksum = GenerateChecksum(userName, password, info.Privilege, info.CreatedAt);
                    return checksum.Equals(info.Checksum);
                }
                privilege = Token.guest;
                return false;
            }

            public static bool ValidateAccess(string database) {
                if (Meta.CurrentPrivilege == Token.admin) {
                    return true;
                }

                var userInfo = Load(userfile);
                if (userInfo.TryGetValue(Meta.CurrentUser, out var info)) {
                    if (info == null) {
                        userInfo.Remove(Meta.CurrentUser);
                        Meta.CurrentUser = Token.guest;
                        Save(userfile, userInfo);
                        return false;
                    }
                    var databases = info.Database;
                    return databases.Contains(database);
                } 
                // User does not exist or has no access to the database
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
        }
    }
}
