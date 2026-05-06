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
            {"silent", $"{isSilent}"}
        };
    }
    
    public static void CreateContentMessage(string msg, bool silent)
    {
        ContextMessage cMSG = new ContextMessage(msg, silent);
        NetworkClient.SendString(JSON.ToJson(cMSG.message));
    }

}
