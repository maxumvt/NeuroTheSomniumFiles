namespace NeuroSomniumFilesController;

using System.Collections.Generic;
using WebSocketSharp;
using System;
using UnityEngine.UI;
using UnityEngine;
using BepInEx;
using TMPro;

[BepInPlugin("com.maxum.dialoglogger", "NeuroSomniumFiles", "1.0.0")]
public class MyPlugin : BaseUnityPlugin
{
    private AgentController agent;

    void Awake()
    {
        var network = new NetworkClient();
        var actionExecutor = new ActionExecutor();
        var actions = new ActionRegistry(actionExecutor);
        var observations = new ObservationProvider();

        agent = new AgentController(network, observations, actions, actionExecutor);

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
    private ActionExecutor actexe;
    public AgentController(NetworkClient net, ObservationProvider obs, ActionRegistry act, ActionExecutor ae)
    {
        network = net;
        observations = obs;
        actions = act;
        actexe = ae;
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
        CharacterSpeaking(searchAllowed);
        DescriptionText(searchAllowed);
        LookChoicesOptions(searchAllowed);
    }

    public void CharacterSpeaking(bool allowSearch)
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
                ContextMessage cMsg = new ContextMessage($"{nameText} says: {dialogueText}", false);
                OnBannerText?.Invoke(cMsg.ToJson());
            }
        }
    }
    public void DescriptionText(bool allowSearch)
    {
        if (allowSearch && descriptionDialogue == null) {  descriptionDialogue = GameObject.Find("$Root/UICanvas/ScreenScaler/UIOff1/PanelNode/NarrationWindow/GameObject/Background/Text")?.GetComponent<TextMeshProUGUI>(); }
        if (descriptionDialogue != null)
        {
            string descrText = descriptionDialogue.text;
            // ERROR Clicking description type windows with one line only, can cause the description not to be logged again. This is bad feedback and needs some kind of solution
            if (!string.IsNullOrEmpty(descrText) && (descrText != descriptionLastline))
            {
                descriptionLastline = descrText;
                ContextMessage cMsg = new ContextMessage($"Description text: {descrText}", false);
                OnBannerText?.Invoke(cMsg.ToJson());
            }
        }
    }
    public void LookChoicesOptions(bool allowSearch)
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
        var termText = lookChoices.transform.Find("Term/Background/Text")?.GetComponent<TextMeshProUGUI>().text;
        currentTerm = termText ?? "";
        ContextMessage cMsg = new ContextMessage($"Looking at {currentTerm}", false);
        OnTermChange?.Invoke(cMsg.ToJson());
        
        CurrentOptions["look_at_term"] = $"Look at {currentTerm}";


        bool buttonUp = lookChoices.transform.Find("SelectU").gameObject.activeSelf;
        if (buttonUp)
        {
            string buttonUpText = lookChoices.transform.Find("SelectU/Background/Text")?.GetComponent<TextMeshProUGUI>().text;
            CurrentOptions["button_up"] = buttonUpText;
        }

        bool buttonDown = lookChoices.transform.Find("SelectD").gameObject.activeSelf;
        if (buttonDown)
        {
            string buttonDownText = lookChoices.transform.Find("SelectD/Background/Text")?.GetComponent<TextMeshProUGUI>().text;
            CurrentOptions["button_down"] = buttonDownText;
        }

        bool buttonLeft = lookChoices.transform.Find("SelectL").gameObject.activeSelf;
        if (buttonLeft)
        {
            string buttonLeftText = lookChoices.transform.Find("SelectL/Background/Text")?.GetComponent<TextMeshProUGUI>().text;
            CurrentOptions["button_left"] = buttonLeftText;
        }

        bool buttonRight = lookChoices.transform.Find("SelectR").gameObject.activeSelf;
        if (buttonRight)
        {
            string buttonRightText = lookChoices.transform.Find("SelectR/Background/Text")?.GetComponent<TextMeshProUGUI>().text;
            CurrentOptions["button_right"] = buttonRightText;
        }

        bool buttonZoom = lookChoices.transform.Find("Zoom").gameObject.activeSelf;
        if (buttonZoom)
        {
            CurrentOptions["zoom_button"] = $"Zoom into {termText}";
        }
        
        bool buttonThermo = lookChoices.transform.Find("Thermo").gameObject.activeSelf;
        if (buttonThermo)
        {
            CurrentOptions["thermo_button"] = $"Thermo vision on {termText}";
        }
        
        bool buttonXray = lookChoices.transform.Find("XRay").gameObject.activeSelf;
        if (buttonXray)
        {
            CurrentOptions["xray_button"] = $"XRay vision on {termText}";
        }
        
        bool buttonNV = lookChoices.transform.Find("NV").gameObject.activeSelf;
        if (buttonNV)
        {
            CurrentOptions["night_vision_button"] = $"Night vision on {termText}";
        }
        
        bool buttonZoomThermo = lookChoices.transform.Find("ZoomThermo").gameObject.activeSelf;
        if (buttonZoomThermo)
        {
            CurrentOptions["zoom_thermo_button"] = $"Thermo vision and zoom into {termText}";
        }
        
        bool buttonZoomXray = lookChoices.transform.Find("ZoomXRay").gameObject.activeSelf;
        if (buttonZoomXray)
        {
            CurrentOptions["zoom_xray_button"] = $"XRay vision and zoom into {termText}";
        }
        
        bool buttonZoomNV = lookChoices.transform.Find("ZoomNV").gameObject.activeSelf;
        if (buttonZoomNV)
        {
            CurrentOptions["zoom_night_vision_button"] = $"Night vision and zoom into {termText}";
        }

        // emit event
        OnLookChoicesUpdated?.Invoke(CurrentOptions);
    }
}

public class ActionRegistry
{
    public ActionExecutor AE;

    public ActionRegistry(ActionExecutor actexe)
    {
        this.AE = actexe;
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
            //System.IO.File.AppendAllText("D:\\temp\\ws_log.txt", key + "\n");
            if (key == action_name)
            {
                // send success
                ActionResultMessage ARMs = new ActionResultMessage(id, true);
                OnResultMessageCreated?.Invoke(ARMs.ToJson());
                
                // call unregister
                Unregister();
                
                // call execute
                AE.QueueAction(action_name);
                return;
            }
        }

        // else only send error back
        ActionResultMessage ARMf = new ActionResultMessage(id, false);
        OnResultMessageCreated?.Invoke(ARMf.ToJson());
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
    public void QueueAction(string action_name)
    {
        System.IO.File.AppendAllText("D:\\temp\\ws_log.txt", $"This is hopefully called on first. Then I just have to figure out what is not going well {action_name}" + "\n");
        switch (action_name)
        {
            case "button_up":
                GameObject up_input = GameObject.Find("$Root/GameController");
                var up_variable = up_input.GetComponent("InputProc");
                var up_pad_states = up_variable.GetType().GetField("padstates", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(up_variable);
                var up_dict = (System.Collections.IDictionary)up_pad_states;
                var up_button = up_dict["BUTTON_WASD_DPAD_UP"];
                var up_button_down = up_button.GetType().GetField("down", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                up_button_down.SetValue(up_button, true);
                break;
            case "button_down":
                GameObject down_input = GameObject.Find("$Root/GameController");
                var down_variable = down_input.GetComponent("InputProc");
                var down_pad_states = down_variable.GetType().GetField("padstates", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(down_variable);
                var down_dict = (System.Collections.IDictionary)down_pad_states;
                var down_button = down_dict["BUTTON_WASD_DPAD_DOWN"];
                var down_button_down = down_button.GetType().GetField("down", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                down_button_down.SetValue(down_button, true);
                break;
            case "button_left":
                GameObject left_input = GameObject.Find("$Root/GameController");
                var left_variable = left_input.GetComponent("InputProc");
                var left_pad_states = left_variable.GetType().GetField("padstates", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(left_variable);
                var left_dict = (System.Collections.IDictionary)left_pad_states;
                var left_button = left_dict["BUTTON_WASD_DPAD_LEFT"];
                var left_button_down = left_button.GetType().GetField("down", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                left_button_down.SetValue(left_button, true);
                break;
            case "button_right":
                GameObject right_input = GameObject.Find("$Root/GameController");
                var right_variable = right_input.GetComponent("InputProc");
                var right_pad_states = right_variable.GetType().GetField("padstates", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(right_variable);
                var right_dict = (System.Collections.IDictionary)right_pad_states;
                var right_button = right_dict["BUTTON_WASD_DPAD_RIGHT"];
                var right_button_down = right_button.GetType().GetField("down", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                right_button_down.SetValue(right_button, true);
                break;
            case "look_at_term":
                GameObject x_input = GameObject.Find("$Root/GameController");
                var x_variable = x_input.GetComponent("InputProc");
                var x_pad_states = x_variable.GetType().GetField("padstates", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(x_variable);
                var x_dict = (System.Collections.IDictionary)x_pad_states;
                var x_button = x_dict["BUTTON_X"];
                var x_button_down = x_button.GetType().GetField("down", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                x_button_down.SetValue(x_button, true);
                break;
            case "zoom_button":
                GameObject zoom_input = GameObject.Find("$Root/GameController");
                var zoom_variable = zoom_input.GetComponent("InputProc");
                var zoom_pad_states = zoom_variable.GetType().GetField("padstates", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(zoom_variable);
                var zoom_dict = (System.Collections.IDictionary)zoom_pad_states;
                var zoom_button = zoom_dict["BUTTON_LSTICK"];
                var zoom_button_down = zoom_button.GetType().GetField("down", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                zoom_button_down.SetValue(zoom_button, true);
                break;
            case "thermo_button":
                // simulate button up
                break;
            case "night_vision_button":
                // simulate button up
                break;
            case "xray_button":
                // simulate button up
                break;
            case "zoom_thermo_button":
                // simulate button up
                break;
            case "zoom_xray_button":
                // simulate button up
                break;
            case "zoom_night_vision_button":
                // simulate button up
                break;
            
        }
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
public class ObservationData
{
    public string target;
    public string entity;
    public string text;
}

public class NeuroMessage
{
    public string command = "startup";
    public string game = "AI Somnium Files";

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