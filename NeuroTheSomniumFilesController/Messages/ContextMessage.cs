namespace NeuroTheSomniumFiles;

public class ContextMessage : NeuroMessage
{
    public string message;
    public bool silent;

    public ContextMessage(string msg, bool isSilent)
    {
        this.command = "context";
        this.message = msg;
        this.silent = isSilent;
    }

    public string ToJson()
    {
        return "{"
            + "\"command\":\"" + command + "\","
            + "\"game\":\"" + game + "\","
            + "\"data\":{"
            + "\"message\":\"" + message + "\","
            + "\"silent\":" + (silent ? "true" : "false")
            + "}"
            + "}";
    }
}
