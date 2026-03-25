namespace NeuroTheSomniumFiles;

using System.Collections.Generic;


public class ActionUnregisterMessage : NeuroMessage
{
    public ActionUnregisterMessage(List<BaseAction> actions)
    {
        this.message["command"] = "actions/unregister";

        List<string> action_names = new List<string>();
        foreach (BaseAction act in actions)
        {
            action_names.Add(act.name);
        }

        this.message["data"] = new Dictionary<string, object>()
        {
            {"action_names", action_names },
        };
        
    }
}