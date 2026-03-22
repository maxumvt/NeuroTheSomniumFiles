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
    
    public Dictionary<string,string> actions = new Dictionary<string, string>();

    public void Register(Dictionary<string,string> acts)
    {
        actions = new Dictionary<string, string>(acts);
        OnUpdateActionList?.Invoke(ToJsonRegister());
        OnUpdateActionList?.Invoke(ToJsonRegisterForce());
    }

    public void Unregister()
    {
        if (actions.Count == 0) return;
        OnUpdateActionList?.Invoke(ToJsonUnRegister());
        actions.Clear();
    }

    public void Validate(string json)
    {
        Debug.Log($"this is received in ActionRegistery: {json}");
        string id;
        string action_name;
        string command;

        // extract id and action_name
        command = JSON.ExtractJsonValue(json, "command");
        if (command != "action") return;
        id = JSON.ExtractJsonValue(json, "id");
        action_name = JSON.ExtractJsonValue(json, "name");
        
        // check valid
        foreach (var key in actions.Keys)
        {
            if (key == action_name)
            {
                ActionResultMessage ARMs = new ActionResultMessage(id, true);
                OnResultMessageCreated?.Invoke(ARMs.ToJson()); // send success
                
                Unregister(); // call unregister
                
                AE.ExecuteAction(action_name); // call execute
                return;
            }
        }
        
        ActionResultMessage ARMf = new ActionResultMessage(id, false);
        OnResultMessageCreated?.Invoke(ARMf.ToJson()); // else send error back
    }

    public string ToJsonRegister()
    {
        // build JSON array of actions
        string actionsJson = "";
        int i = 0;
        foreach (var kv in actions)
        {
            if (i > 0) actionsJson += ",";
            actionsJson += "{"
                        + $"\"name\":\"{kv.Key}\","
                        + $"\"description\":\"{kv.Value}\""
                        + "}";
            i++;
        }

        return "{"
           + "\"command\":\"actions/register\","
           + "\"game\":\"AI Somnium Files\","
           + "\"data\":{"
           + $"\"actions\":[{actionsJson}]"
           + "}"
           + "}";
    }
    
    public string ToJsonRegisterForce()
    {
        // build JSON array of actions
        string actionsJson = "";
        int i = 0;
        foreach (var kv in actions)
        {
            if (i > 0) actionsJson += ",";
            actionsJson += $"\"{kv.Key}\"";
            i++;
        }

        return "{"
            + "\"command\":\"actions/force\","
            + $"\"game\":\"AI Somnium Files\","
            + "\"data\":{"
            + $"\"query\":\"do something\","
            + $"\"action_names\":[{actionsJson}],"
            + $"\"state\":\"its a test\","
            + "\"ephemeral_context\":false,"
            + "\"priority\":\"low\""
            + "}"
            + "}";
    }

    public string ToJsonUnRegister()
    {
        // build JSON array of actions
        string actionsJson = "";
        int i = 0;
        foreach (var kv in actions)
        {
            if (i > 0) actionsJson += ",";
            actionsJson += $"\"{kv.Key}\"";
            i++;
        }

        return "{"
           + "\"command\":\"actions/unregister\","
           + "\"game\":\"AI Somnium Files\","
           + "\"data\":{"
           + $"\"action_names\":[{actionsJson}]"
           + "}"
           + "}";
    }
}
