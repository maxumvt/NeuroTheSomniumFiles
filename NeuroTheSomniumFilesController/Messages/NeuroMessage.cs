using System.Collections.Generic;
using System.Linq;

namespace NeuroTheSomniumFiles;

public class NeuroMessage
{
    public string command = "startup";
    public string game = "AI The Somnium Files";
    public Dictionary<string, string> message = new Dictionary<string, string>()
    {
        {"command", "startup"},
        {"game","AI The Somnium Files"},

    };

    public string StartupToJson()
    {
        //message.Concat(message);
        return "{"
            + "\"command\":\"" + command + "\","
            + "\"game\":\"" + game + "\""
            + "}";
    }
}