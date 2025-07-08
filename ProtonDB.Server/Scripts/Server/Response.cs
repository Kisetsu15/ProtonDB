namespace ProtonDB.Server {
    /// <summary>
    /// Represents a standard response object used by the server to communicate status, messages, and results.
    /// </summary>
    public class Response {
        /// <summary>
        /// Gets or sets the status of the response.
        /// Default is "ok".
        /// </summary>
        public string Status { get; set; } = "ok";

        /// <summary>
        /// Gets or sets an optional message providing additional information about the response.
        /// </summary>
        public string Message { get; set; } = "";

        /// <summary>
        /// Gets or sets the result data associated with the response.
        /// This is typically an array of strings, or null if no result is present.
        /// </summary>
        public string[]? Result { get; set; }
    }
}