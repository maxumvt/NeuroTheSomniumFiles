namespace NeuroSomniumFilesController;

using System.Collections.Generic;
using WebSocketSharp;
using System;
using UnityEngine.UI;
using UnityEngine;
using BepInEx;
using TMPro;

[BepInPlugin("com.maxum.dialoglogger", "NeuroTheSomniumFiles", "1.0.0")]
public class MyPlugin : BaseUnityPlugin
{
    private AgentController agent;

    void Awake()
    {
        var network = new NetworkClient();
        var actionExecutor = new ActionExecutor();
        var actions = new ActionRegistry(actionExecutor);
        var observations = new ObservationProvider();

        agent = new AgentController(network, observations, actions);
        agent.Initialize();
    }

    void Update()
    {
        agent.Tick();
    }
}


// Functionaolity
public class AgentController
{
    private NetworkClient network;
    private ObservationProvider observations;
    private ActionRegistry actions;
    public AgentController(NetworkClient net, ObservationProvider obs, ActionRegistry act)
    {
        network = net;
        observations = obs;
        actions = act;
    }

    private float searchTimer = 0;
    private bool searchAllowed = true;

    public void Initialize()
    {
        network.OnMessageReceived += actions.Validate;
        observations.OnBannerText += network.SendString;
        observations.OnLookChoicesUpdated += actions.Register;
        observations.OnTermChange += network.SendString;
        actions.OnUpdateActionList += network.SendString;
        actions.OnResultMessageCreated += network.SendString;
        
        network.Connect();
        NeuroMessage startUpMsg = new NeuroMessage();
        network.SendString(startUpMsg.StartupToJson());

    }

    public void Tick()
    {
        searchTimer += Time.deltaTime;
        if ( searchTimer > 1f) { searchAllowed = true; searchTimer = 0f; }
        else searchAllowed = false;
        
        observations.Collect(searchAllowed);
    }
}

public class ObservationProvider
{
    public event Action<string> OnTermChange;
    public event Action<string> OnBannerText;
    public event Action<Dictionary<string, string>> OnLookChoicesUpdated;

    public RawImage characterNamePlate;
    public TextMeshProUGUI characterDialogue;
    public TextMeshProUGUI descriptionDialogue;
    public string dialogueLastline;
    public string descriptionLastline;

    public GameObject lookChoices;
    public bool interactLook = false;
    public string currentTerm = "";
    public Dictionary<string, string> CurrentOptions = new Dictionary<string, string>();

    public void Collect(bool searchAllowed)
    {
        MessageWindow(searchAllowed);
        NarrationWindow(searchAllowed);
        //EventNarrationWindow(searchAllowed)
        //EventMessageWindow(searchAllowed)
        //SubtitleWindow(searchAllowed)
        InvestigationInteractOptions(searchAllowed);
        //SomniumInteractOptions(searchAllowed)
    }

    public void MessageWindow(bool allowSearch)
    {
        if (allowSearch && characterNamePlate == null) {characterNamePlate = GameObject.Find("$Root/UICanvas/ScreenScaler/UIOff1/PanelNode/MessageWindow/Rig/Name/Text")?.GetComponent<RawImage>();} // Necessary for finding the object
        if (allowSearch && characterDialogue == null) {characterDialogue = GameObject.Find("$Root/UICanvas/ScreenScaler/UIOff1/PanelNode/MessageWindow/Rig/Background/Text")?.GetComponent<TextMeshProUGUI>();} // Necessary for finding the object
        
        if (characterNamePlate != null && characterDialogue != null)
        {
            string nameText = characterNamePlate.mainTexture.name;
            string dialogueText = characterDialogue.text;

            if (!string.IsNullOrEmpty(dialogueText) && dialogueLastline != dialogueText)
            {
                dialogueLastline = dialogueText;
                SendBannerText($"{nameText} says: {dialogueText}", false);
                // ContextMessage cMsg = new ContextMessage($"{nameText} says: {dialogueText}", false);
                // OnBannerText?.Invoke(cMsg.ToJson());
            }
        }
    }
    public void NarrationWindow(bool allowSearch)
    {
        if (allowSearch && descriptionDialogue == null)  descriptionDialogue = GameObject.Find("$Root/UICanvas/ScreenScaler/UIOff1/PanelNode/NarrationWindow/GameObject/Background/Text")?.GetComponent<TextMeshProUGUI>();
        if (descriptionDialogue != null)
        {
            string descrText = descriptionDialogue.text;
            // ERROR Clicking description type windows with one line only, can cause the description not to be logged again. This is bad feedback and needs some kind of solution
            if (!string.IsNullOrEmpty(descrText) && (descrText != descriptionLastline))
            {
                descriptionLastline = descrText;
                SendBannerText($"Description text: {descrText}", false);
                // ContextMessage cMsg = new ContextMessage($"Description text: {descrText}", false);
                // OnBannerText?.Invoke(cMsg.ToJson());
            }
        }
    }
    public void InvestigationInteractOptions(bool allowSearch)
    {
        if (lookChoices == null)
        {
            if (!allowSearch) return;

            lookChoices = GameObject.Find("$Root/CommandCanvas/ScreenScaler/Command/Scale");
            if (lookChoices == null) return;
        }

        bool lookActive = lookChoices.transform.Find("Look").gameObject.activeSelf;

        if (interactLook == lookActive) return; // no change
        interactLook = lookActive;

        CurrentOptions.Clear();
        if (!lookActive)
        {
            currentTerm = "";
            return;
        }

        // update term
        string termText = lookChoices.transform.Find("Term/Background/Text")?.GetComponent<TextMeshProUGUI>().text;
        currentTerm = termText ?? "";
        ContextMessage cMsg = new ContextMessage($"Looking at {currentTerm}", false);
        OnTermChange?.Invoke(cMsg.ToJson());
        
        CurrentOptions["look_at_term"] = $"Look at {currentTerm}";

        // Checks if button is and is active
        AddButtonIfActive("SelectU", "button_up");
        AddButtonIfActive("SelectD", "button_down");
        AddButtonIfActive("SelectL", "button_left");
        AddButtonIfActive("SelectR", "button_right");
        AddButtonIfActive("Zoom", "zoom_button", $"Zoom into {termText}");
        AddButtonIfActive("Thermo", "thermo_button", $"Thermo vision on {termText}");
        AddButtonIfActive("XRay", "xray_button", $"XRay vision on {termText}");
        AddButtonIfActive("NV", "night_vision_button", $"Night vision on {termText}");
        AddButtonIfActive("ZoomThermo", "zoom_thermo_button", $"Thermo vision and zoom into {termText}");
        AddButtonIfActive("ZoomXRay", "zoom_xray_button", $"XRay vision and zoom into {termText}");
        AddButtonIfActive("ZoomNV", "zoom_night_vision_button", $"Night vision and zoom into {termText}");

        // emit event
        OnLookChoicesUpdated?.Invoke(CurrentOptions);
    }

    private void AddButtonIfActive(string buttonName, string KeyPrefix, string customText="")
    {
        var buttonObj = lookChoices.transform.Find(buttonName)?.gameObject; // Find button object
        if (buttonObj != null && buttonObj.activeSelf) // Make sure the button exists AND is active
        {
            var textComp = lookChoices.transform.Find(buttonName + "/Background/Text")?.GetComponent<TextMeshProUGUI>(); // Store the button GameObject
            if ( textComp != null) CurrentOptions[KeyPrefix] = textComp.text; // Use the button text as action description
            else CurrentOptions[KeyPrefix] = customText; // Use custom text as action description
        }
    }
    private void SendBannerText(string message, bool isSilent)
    {
        ContextMessage cMsg = new ContextMessage(message, isSilent);
        OnBannerText?.Invoke(cMsg.ToJson());
    }
}

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
        JSON jsonHelper = new JSON();
        command = jsonHelper.ExtractJsonValue(json, "command");
        if (command != "action") return;
        id = jsonHelper.ExtractJsonValue(json, "id");
        action_name = jsonHelper.ExtractJsonValue(json, "name");
        
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

public class NetworkClient
{
    private WebSocket ws;
    public event Action<string> OnMessageReceived;

    public void Connect()
    {
        ws = new WebSocket("ws://localhost:8000");

        ws.OnOpen += OnOpen;
        ws.OnMessage += OnMessage;
        ws.OnError += OnError;
        ws.OnClose += OnClose;

        ws.Connect();
    }

    private void OnOpen(object sender, System.EventArgs e)
    {
        Debug.Log("[WebSocket] Connected");
    }

    private void OnMessage(object sender, MessageEventArgs e)
    {
        try
        {
            System.IO.File.AppendAllText("D:\\temp\\ws_log.txt", e.Data + "\n");
        }
        catch {}

        Debug.Log("[WebSocket] Received: " + e.Data);
        string text = e.Data;
        OnMessageReceived?.Invoke(text);
    }

    private void OnError(object sender, ErrorEventArgs e)
    {
        Debug.Log("[WebSocket] Error: " + e.Message);
    }

    private void OnClose(object sender, CloseEventArgs e)
    {
        Debug.Log("[WebSocket] Closed: " + e.Code);
    }

    public void SendString(string json)
    {
        if (ws != null && ws.IsAlive)
        {
            ws.Send(json);
        }
    }
}

public class ActionExecutor
{
    public void ExecuteAction(string action_name)
    {
        switch (action_name)
        {
            case "button_up":
                PressButton("BUTTON_WASD_DPAD_UP");
                break;
            case "button_down":
                PressButton("BUTTON_WASD_DPAD_DOWN");
                break;
            case "button_left":
                PressButton("BUTTON_WASD_DPAD_LEFT");
                break;
            case "button_right":
                PressButton("BUTTON_WASD_DPAD_RIGHT");
                break;
            case "look_at_term":
                PressButton("Submit");
                break;
            case "zoom_button":
                PressButton("BUTTON_LSTICK");
                break;
            case "thermo_button":
                PressButton("BUTTON_LSTICK"); // simulate thermo, but uncertain button
                break;
            case "night_vision_button":
                PressButton("BUTTON_LSTICK"); // simulate night vision, but uncertain button
                break;
            case "xray_button":
                PressButton("BUTTON_LSTICK");
                break;
            case "zoom_thermo_button":
                PressButton("BUTTON_LSTICK"); // simulate zoom thermo, but uncertain button
                break;
            case "zoom_xray_button":
                PressButton("BUTTON_LSTICK"); // simulate zoom xray, but uncertain button
                break;
            case "zoom_night_vision_button":
                PressButton("BUTTON_LSTICK"); // simulate zoom night vision, but uncertain button
                break;
            
        }
    }

    private void PressButton(string buttonKey)
    {
        Component inputProc = GameObject.Find("$Root/GameController").GetComponent("InputProc"); // Gets InputProc
        var pad_states = inputProc.GetType().GetField("padstates", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(inputProc); // Gets buttons with their state
        var dict = (System.Collections.IDictionary)pad_states; // Turn it into a dict
        var button = dict[buttonKey]; // Retrieve the current key's state
        var button_down = button.GetType().GetField("down", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public); // Get the "down" field of the key's state
        button_down.SetValue(button, true); // Set the "down" state of the button to true
    }
}


// Helper function
public class JSON
{
    public string ExtractJsonValue(string json, string key)
    {
        string pattern = $"\"{key}\":\"(.*?)\"";
        System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(json, pattern);
        if (match.Success) return match.Groups[1].Value;
        return "";
    }
}


// Resources
// A class for cleaning up text send to Neuro
//  Example: transform ui_main_name_c01 into Kaname Date
//  Example: remove <width></width> from texts
//  Example: remove <color> and change the text to telepathy between Aiba and Date

public class NeuroMessage
{
    public string command = "startup";
    public string game = "AI The Somnium Files";

    public string StartupToJson()
    {
        return "{"
            + "\"command\":\"" + command + "\","
            + "\"game\":\"" + game + "\""
            + "}";
    }
}

public class ContextMessage : NeuroMessage
{
    public string message;
    public bool silent;

    public ContextMessage(string msg, bool isSilent)
    {
        this.command = "context";
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