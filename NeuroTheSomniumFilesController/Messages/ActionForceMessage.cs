namespace NeuroTheSomniumFiles;

using System.Collections.Generic;
using System.Linq;

public class ActionforceMessage : NeuroMessage
{

    private List<string> action_names = new List<string>();

    public ActionforceMessage(List<BaseAction> actions, string stat = "", string quer = "", bool ephem = false, string prior = "low")
    {
        this.message["command"] = "actions/force";

        foreach (BaseAction act in actions)
        {
            action_names.Add(act.name);
        }

        this.message["data"] = new Dictionary<string, object>()
        {
            {"state", stat },
            {"query", quer },
            {"ephemeral_context", ephem },
            {"priority", prior },
            {"action_names", action_names },
        };
        
    }
    public static void CreateForceMessage(List<BaseAction> actions, string stat = "", string quer = "", bool ephem = false, string prior = "low")
    {
        ActionforceMessage cMSG = new ActionforceMessage(actions);
        NetworkClient.SendString(JSON.ToJson(cMSG.message));
    }
}