namespace NeuroTheSomniumFiles;

using System.Collections.Generic;


public class ActionResultMessage : NeuroMessage
{
    public ActionResultMessage(string incoming_id, bool incoming_action_success)
    {
        this.message["command"] = "action/result";
        this.message["data"] = new Dictionary<string, string>()
        {
            {"id",$"{incoming_id}"},
            {"success", $"{incoming_action_success}"},
            {"message", null},
        };
    }
}