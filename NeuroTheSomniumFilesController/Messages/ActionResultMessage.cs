namespace NeuroTheSomniumFiles;


public class ActionResultMessage : NeuroMessage
{
    public string id;
    public bool action_success;
    public ActionResultMessage(string incoming_id, bool incoming_action_success)
    {
        this.command = "action/result";
        this.id = incoming_id;
        this.action_success = incoming_action_success;
    }

    public string ToJson()
    {
        return "{"
            + $"\"command\":\"{command}\","
            + "\"data\":{"
            + $"\"id\":\"{id}\","
            + $"\"success\":{action_success.ToString().ToLower()}"
            + "}"
            + "}";
    }
}