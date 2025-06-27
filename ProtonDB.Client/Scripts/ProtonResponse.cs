namespace ProtonDB.Client;
public class ProtonResponse {
    public string Status { get; set; } = "ok";
    public string Message { get; set; } = "";
    public string[]? Result { get; set; }
}

