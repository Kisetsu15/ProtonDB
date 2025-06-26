namespace ProtonDB.Server {
    namespace Core {
        public record ProfileInfo(
            string Checksum,
            string Privilege,
            string CreatedAt,
            List<string> Database
        );

        public class HashMap {
            private readonly Dictionary<string, ProfileInfo> _users = [];

            public IEnumerable<string> UserNames => _users.Keys;
            public IEnumerable<ProfileInfo> Infos => _users.Values;

            public bool Add(string userName, ProfileInfo info) {
                return _users.TryAdd(userName, info);
            }

            public bool Remove(string userName) {
                return _users.Remove(userName);
            }

            public bool TryGetValue(string userName, out ProfileInfo? profile) {
                return _users.TryGetValue(userName, out profile);
            }

            public ProfileInfo? this[string userName] =>
                _users.TryGetValue(userName, out var profile) ? profile : null;
        }
    }
}
