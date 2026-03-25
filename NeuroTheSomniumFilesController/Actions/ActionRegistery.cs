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
    
    // public Dictionary<string,string> actions = new Dictionary<string, string>();
    public List<BaseAction> actions = new List<BaseAction>();

    public void Register(List<BaseAction> acts)
    {
        ActionRegisterMessage ARM = new ActionRegisterMessage(acts);
        OnUpdateActionList?.Invoke(JSON.ToJson(ARM.message));
        Debug.Log($"this is the dictionary for the actions in order: {actions}");
        // actions = new Dictionary<string, string>(acts);
        // OnUpdateActionList?.Invoke(ToJsonRegister());
        // OnUpdateActionList?.Invoke(ToJsonRegisterForce());
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

    // public string ToJsonRegister()
    // {
    //     // build JSON array of actions
    //     string actionsJson = "";
    //     int i = 0;
    //     foreach (var kv in actions)
    //     {
    //         if (i > 0) actionsJson += ",";
    //         actionsJson += "{"
    //                     + $"\"name\":\"{kv.Key}\","
    //                     + $"\"description\":\"{kv.Value}\""
    //                     + "}";
    //         i++;
    //     }

    //     return "{"
    //        + "\"command\":\"actions/register\","
    //        + "\"game\":\"AI Somnium Files\","
    //        + "\"data\":{"
    //        + $"\"actions\":[{actionsJson}]"
    //        + "}"
    //        + "}";
    // }
    
    // public string ToJsonRegisterForce()
    // {
    //     // build JSON array of actions
    //     string actionsJson = "";
    //     int i = 0;
    //     foreach (var kv in actions)
    //     {
    //         if (i > 0) actionsJson += ",";
    //         actionsJson += $"\"{kv.Key}\"";
    //         i++;
    //     }

    //     return "{"
    //         + "\"command\":\"actions/force\","
    //         + $"\"game\":\"AI Somnium Files\","
    //         + "\"data\":{"
    //         + $"\"query\":\"do something\","
    //         + $"\"action_names\":[{actionsJson}],"
    //         + $"\"state\":\"its a test\","
    //         + "\"ephemeral_context\":false,"
    //         + "\"priority\":\"low\""
    //         + "}"
    //         + "}";
    // }

    // public string ToJsonUnRegister()
    // {
    //     // build JSON array of actions
    //     string actionsJson = "";
    //     int i = 0;
    //     foreach (var kv in actions)
    //     {
    //         if (i > 0) actionsJson += ",";
    //         actionsJson += $"\"{kv.Key}\"";
    //         i++;
    //     }

    //     return "{"
    //        + "\"command\":\"actions/unregister\","
    //        + "\"game\":\"AI Somnium Files\","
    //        + "\"data\":{"
    //        + $"\"action_names\":[{actionsJson}]"
    //        + "}"
    //        + "}";
    // }
}
