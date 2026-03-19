namespace NeuroSomniumFiles;

using System.Collections.Generic;

public class NeuroMessage
{
    public string command { get; set; }
    public string game = "AI Somnium Files";
}

public class Action
{
    public string name;
    public string description;

    public Action(string n, string d)
    {
        name = n;
        description = d;
    }
}

public class ContextMessage : NeuroMessage
{
    public string message;
    public bool silent;

    public ContextMessage(string msg, bool isSilent)
    {
        this.command = "Context";
        this.message = msg;
        this.silent = isSilent;
    }

    public string ToJson()
    {
        return "{"
            + "\"command\":\"" + command + "\","
            + "\"game\":\"" + game + "\","
            + "\"data\":{"
            + "\"message\":\"" + message + "\","
            + "\"silent\":" + (silent ? "true" : "false")
            + "}"
            + "}";
    }
}

public class RegisterActionsMessage : NeuroMessage
{
    public List<Action> actions;

    public RegisterActionsMessage(List<Action> acts)
    {
        command = "actions/register";
        actions = acts;
    }

    public string ToJson()
    {
        string actionsJson = "";
        for (int i = 0; i < actions.Count; i++)
        {
            if (i > 0) actionsJson += ",";
            actionsJson += "{"
                + "\"name\":\"" + actions[i].name + "\","
                + "\"description\":\"" + actions[i].description + "\""
                + "}";
        }

        return "{"
            + "\"command\":\"" + command + "\","
            + "\"game\":\"" + game + "\","
            + "\"data\":{"
            + "\"actions\":[" + actionsJson + "]"
            + "}"
            + "}";
    }
}

public class UnregisterActionsMessage : NeuroMessage
{
    public List<string> actionNames;

    public UnregisterActionsMessage()
    {
        command = "actions/unregister";
        actionNames = new List<string>();
    }

    public void setActionNames(List<Action> acts)
    {
        actionNames.Clear();
        
        for (int i = 0; i < acts.Count; i++)
        {
            actionNames.Add(acts[i].name);
        }
    }
    public List<string> getActionNames()
    {
        return actionNames;
    }

    public string ToJson()
    {
        string namesJson = "";
        for (int i = 0; i < actionNames.Count; i++)
        {
            if (i > 0) namesJson += ",";
            namesJson += "\"" + actionNames[i] + "\"";
        }

        return "{"
            + "\"command\":\"" + command + "\","
            + "\"game\":\"" + game + "\","
            + "\"data\":{"
            + "\"action_names\":[" + namesJson + "]"
            + "]}"
            + "}";
    }
}

public class ActionResultMessage : NeuroMessage
{
    public string id;
    public bool success;
    public string messageText;

    public ActionResultMessage(string actionId, bool ok, string msg)
    {
        command = "action/result";
        id = actionId;
        success = ok;
        messageText = msg;
    }

    public string ToJson()
    {
        return "{"
            + "\"command\":\"" + command + "\","
            + "\"game\":\"" + game + "\","
            + "\"data\":{"
            + "\"id\":\"" + id + "\","
            + "\"success\":" + (success ? "true" : "false") + ","
            + "\"message\":\"" + messageText + "\""
            + "}"
            + "}";
    }
}