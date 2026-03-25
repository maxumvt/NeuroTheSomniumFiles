namespace NeuroTheSomniumFiles;

using System.Collections.Generic;


public class ActionRegisterMessage : NeuroMessage
{
    public ActionRegisterMessage(List<BaseAction> actions)
    {
        this.message["command"] = "actions/register";
        this.message["data"] = new Dictionary<string, object>()
        {
            {"actions", actions },
        };
        
    }
}