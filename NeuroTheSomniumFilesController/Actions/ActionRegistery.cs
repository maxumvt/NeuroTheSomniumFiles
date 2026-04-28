namespace NeuroTheSomniumFiles;

using System;
using System.Collections.Generic;

public class ActionRegistry
{
    public ActionExecutor AE = new ActionExecutor();
    
    public static List<BaseAction> actions = new List<BaseAction>();

    public void Validate(string json)
    {
        string id;
        string action_name;
        string command;

        actions = GameObject_SetActive_Patch.previous_options;

        // extract id and action_name
        command = JSON.ExtractJsonValue(json, "command");
        if (command != "action") return;
        id = JSON.ExtractJsonValue(json, "id");
        action_name = JSON.ExtractJsonValue(json, "name");
        
        // check valid
        foreach (BaseAction action in actions)
        {
            if (action.name == action_name)
            {
                ActionResultMessage ARMs = new ActionResultMessage(id, true);
                NetworkClient.SendString(JSON.ToJson(ARMs.message)); // send success
                
                AE.ExecuteAction(action_name); // call execute
                return;
            }
        }
        
        ActionResultMessage armMsg = new ActionResultMessage(id, false);
        NetworkClient.SendString(JSON.ToJson(armMsg.message)); // else send error back
    }
}
