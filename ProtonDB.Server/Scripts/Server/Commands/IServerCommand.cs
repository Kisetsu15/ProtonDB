namespace ProtonDB.Server {
    public interface IServerCommand {
        Task ExecuteAsync(QuerySession session, StreamWriter writer, Request request);
    }

}