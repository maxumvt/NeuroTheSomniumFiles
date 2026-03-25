namespace NeuroTheSomniumFiles;

using System.Collections.Generic;

public class ContextMessage : NeuroMessage
{
    public ContextMessage(string msg, bool isSilent)
    {
        this.message["command"] = "context";
        this.message["data"] = new Dictionary<string, string>()
        {
            {"message", msg},
            {"isSilent", $"{isSilent}"}
        };
    }

}
