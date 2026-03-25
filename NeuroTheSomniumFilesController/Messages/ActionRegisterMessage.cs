namespace NeuroTheSomniumFiles;

using System.Collections.Generic;


public class ActionRegisterMessage : NeuroMessage
{
    public ActionRegisterMessage(List<Dictionary<string,string>> actions)
    {
        this.message["command"] = "action/result";
        this.message["data"] = new Dictionary<string, object>()
        {
             {"data", {"actions", null }},
        };
        
    }
}