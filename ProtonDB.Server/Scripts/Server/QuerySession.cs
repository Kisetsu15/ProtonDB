using ProtonDB.Server.Core;

namespace ProtonDB.Server {
    public class QuerySession {
        public string? LastQuery { get; set; }
        public string[]? Result { get; set; }
        public bool Debug { get; set; } = false;
        public bool ShouldExit { get; set; } = false;
        public bool IsAuthenticated { get; set; } = false;

        public Profile CurrentProfile { get; set; } = new();

        public string CurrentUser => CurrentProfile.profileName;
        public string CurrentPrivilege => CurrentProfile.profileInfo.Privilege;
        public List<string> CurrentUserDatabases => CurrentProfile.profileInfo.Database;
        public string CurrentDatabase { get; set; } = Meta.defaultDatabase;
    }
}