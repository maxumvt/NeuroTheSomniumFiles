namespace NeuroTheSomniumFiles;

using System;
using System.Collections.Generic;
using UnityEngine;

public class ActionRegistry
{
    public ActionExecutor AE;
    public ActionRegistry(ActionExecutor actexe)
    {
        AE = actexe;
    }

    public event Action<string> OnUpdateActionList;
    public event Action<string> OnResultMessageCreated;
    
    public List<BaseAction> actions = new List<BaseAction>();

    public void Register(List<BaseAction> acts)
    {
        ActionRegisterMessage ARM = new ActionRegisterMessage(acts);
        OnUpdateActionList?.Invoke(JSON.ToJson(ARM.message));

        ActionforceMessage AFM = new ActionforceMessage(acts);
        OnUpdateActionList?.Invoke(JSON.ToJson(AFM.message));
        
        actions = acts;
        Debug.Log($"this is the dictionary for the actions in order: {actions}");
    }

    public void Unregister()
    {
        if (actions.Count == 0) return;
        ActionUnregisterMessage AUM = new ActionUnregisterMessage(actions){};
        OnUpdateActionList?.Invoke(JSON.ToJson(AUM.message));
        actions.Clear();
    }

    public void Validate(string json)
    {
        string id;
        string action_name;
        string command;

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
                OnResultMessageCreated?.Invoke(JSON.ToJson(ARMs.message)); // send success
                
                Unregister(); // call unregister
                
                AE.ExecuteAction(action_name); // call execute
                return;
            }
        }
        
        ActionResultMessage ARMf = new ActionResultMessage(id, false);
        OnResultMessageCreated?.Invoke(JSON.ToJson(ARMf.message)); // else send error back
    }
}
