namespace NeuroTheSomniumFiles;

using System;
using System.Collections.Generic;
using WebSocketSharp;

public class ActionRegistry
{
    public ActionExecutor AE = new ActionExecutor();
    
    public static List<BaseAction> actions_investigation = new List<BaseAction>();
    public static List<BaseAction> actions_somnium = new List<BaseAction>();

    public void Validate(string json)
    {
        string id;
        string action_name;
        string command;

        actions_investigation = Dialogue_SetActive_Patch.GetPreviousActions() ?? new List<BaseAction>();
        actions_somnium = SomniumDialogue_SetActive_Patch.GetPreviousActions() ?? new List<BaseAction>();
        
        // extract id and action_name
        command = JSON.ExtractJsonValue(json, "command");
        if (command != "action") return;
        id = JSON.ExtractJsonValue(json, "id");
        action_name = JSON.ExtractJsonValue(json, "name");

        // check valid investigation
        foreach ( BaseAction action in actions_investigation )
        {
            if (action.name == action_name)
            {
                ActionResultMessage ARMs = new ActionResultMessage(id, true);
                NetworkClient.SendString(JSON.ToJson(ARMs.message)); // send success
                
                AE.ExecuteAction(action_name); // call execute
                return;
            }
        }
        // check valid somnium
        foreach ( BaseAction action in actions_somnium )
        {
            if (action.name == action_name)
            {
                ActionResultMessage ARMs = new ActionResultMessage(id, true);
                NetworkClient.SendString(JSON.ToJson(ARMs.message)); // send success
                
                AE.ExecuteAction(action_name); // call execute

                SomniumDialogue_SetActive_Patch.ResetOptions(); // Unregister the somnium options (Necessary because of the game structure)
                return;
            }
        }

        ActionResultMessage armMsg = new ActionResultMessage(id, false);
        NetworkClient.SendString(JSON.ToJson(armMsg.message)); // else send error back
    }
}
