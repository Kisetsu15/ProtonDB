namespace ProtonDB.Server {
    public class QuerySession {
        public string? LastQuery { get; set; }
        public string[]? Result { get; set; }
        public bool Debug { get; set; } = false;
        public bool ShouldExit { get; set; } = false;
        public bool IsAuthenticated { get;  set; } = false;
    }
}