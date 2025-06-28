namespace ProtonDB.Client;

/// <summary>
/// Represents a request sent to the ProtonDB server, including the command and optional data payload.
/// </summary>
public class ProtonRequest {
    /// <summary>
    /// Gets or sets the command to be executed by the server (e.g., "QUERY", "FETCH", "LOGIN").
    /// </summary>
    public string Command { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional data associated with the command, such as query text or credentials.
    /// </summary>
    public string? Data { get; set; }
}