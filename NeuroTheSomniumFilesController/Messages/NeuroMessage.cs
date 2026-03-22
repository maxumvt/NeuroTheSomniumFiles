namespace NeuroTheSomniumFiles;

public class NeuroMessage
{
    public string command = "startup";
    public string game = "AI The Somnium Files";

    public string StartupToJson()
    {
        return "{"
            + "\"command\":\"" + command + "\","
            + "\"game\":\"" + game + "\""
            + "}";
    }
}