namespace NeuroTheSomniumFiles;

using System.Collections.Generic;


public class ActionforceMessage : NeuroMessage
{
    public ActionforceMessage(List<BaseAction> actions)
    {
        this.message["command"] = "actions/force";

        List<string> action_names = new List<string>();
        foreach (BaseAction act in actions)
        {
            action_names.Add(act.name);
        }

        this.message["data"] = new Dictionary<string, object>()
        {
            {"state", ""},
            {"query", ""},
            {"ephemeral_context", false},
            {"priority", "low"},
            {"action_names", action_names },
        };
        
    }
}