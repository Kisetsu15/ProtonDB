namespace ProtonDB.Client;

/// <summary>
/// Represents a response from the ProtonDB server, including status, message, and optional result data.
/// </summary>
public class ProtonResponse {
    /// <summary>
    /// Gets or sets the status of the response (e.g., "ok" for success, or an error code).
    /// </summary>
    public string Status { get; set; } = "ok";

    /// <summary>
    /// Gets or sets the message associated with the response, such as error details or informational text.
    /// </summary>
    public string Message { get; set; } = "";

    /// <summary>
    /// Gets or sets the result data returned by the server, if any.
    /// </summary>
    public string[]? Result { get; set; }
}