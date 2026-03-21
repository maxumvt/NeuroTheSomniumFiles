namespace NeuroSomniumFilesController;

using System.Collections.Generic;
using WebSocketSharp;
using System;
using System.IO;
using System.Threading;
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
        observations.OnLookDisable += actions.Unregister;
        
        network.Connect();
        NeuroMessage startUpMsg = new NeuroMessage();
        network.SendString(startUpMsg.StartupToJson());

    }

    public void Tick()
    {
        searchTimer += Time.deltaTime;
        if ( searchTimer > 1f) { searchAllowed = true; searchTimer = 0f; }
        else searchAllowed = false;
        
        network.Tick();
        observations.Collect(searchAllowed);
    }
}

public class ObservationProvider
{
    public class DialogueSet
    {
        public RawImage namePlate;
        public TextMeshProUGUI dialogue;
        public string lastLine;
        public string verb;
        public string location;
        public string canvas;
        public DialogueSet(string canvas, string location, string verb = "says")
        {
            this.canvas = canvas;
            this.verb = verb;
            this.location = location;
        }
    } 
    public class TextOnlySet
    {
        public TextMeshProUGUI message;
        public string lastLine;
        public string location;
        public string canvas;
        public TextOnlySet(string canvas, string location)
        {
            this.canvas = canvas;
            this.location = location;
        }
    }
    public class OptionSet
    {
        public GameObject optionsObject;
        public bool interactionActive;
        public Dictionary<string, string> currentOptions = new Dictionary<string, string>();
        public string focusTerm;
    }

    public event Action<string> OnTermChange;
    public event Action<string> OnBannerText;
    public event Action OnLookDisable;
    public event Action<Dictionary<string, string>> OnLookChoicesUpdated;

    // Condition trackers Investigation
    public DialogueSet message = new DialogueSet("UICanvas", "MessageWindow");
    public TextOnlySet narration = new TextOnlySet("UICanvas", "NarrationWindow");
    public DialogueSet eventMessage = new DialogueSet("UICanvas", "EventMessageWindow");
    public TextOnlySet eventNarration = new TextOnlySet("UICanvas", "EventNarrationWindow");
    public DialogueSet flashBackMessage = new DialogueSet("UICanvas", "FlashBackWindow","said");
    public DialogueSet subtitle = new DialogueSet("UICanvas", "SubtitleWindow");
    public TextOnlySet lyrics = new TextOnlySet("UICanvas", "Lyrics");

    // Condition trackers Somnium
    public DialogueSet somniumMessage = new DialogueSet("Canvas (1)", "MessageWindow");
    public TextOnlySet somniumNarration = new TextOnlySet("Canvas (1)", "NarrationWindow");
    public DialogueSet somniumEventMessage = new DialogueSet("Canvas (1)", "EventMessageWindow");
    public TextOnlySet somniumEventNarration = new TextOnlySet("Canvas (1)", "EventNarrationWindow");
    public DialogueSet somniumFlashBackMessage = new DialogueSet("Canvas (1)", "FlashBackWindow", "said");
    public DialogueSet somniumSubtitle = new DialogueSet("Canvas (1)", "SubtitleWindow");
    public TextOnlySet somniumLyrics = new TextOnlySet("Canvas (1)", "Lyrics");

    // Interact options
    public OptionSet investigationInteraction = new OptionSet();
    public OptionSet somniumInteraction = new OptionSet();

    public void Collect(bool searchAllowed)
    {
        SayingExtractor(searchAllowed, message);
        NarrationExtractor(searchAllowed, narration);
        SayingExtractor(searchAllowed, eventMessage);
        NarrationExtractor(searchAllowed, eventNarration);
        SayingExtractor(searchAllowed, flashBackMessage);
        SayingExtractor(searchAllowed, subtitle);
        NarrationExtractor(searchAllowed, lyrics);
        InvestigationInteractOptions(searchAllowed);

        SayingExtractor(searchAllowed, somniumMessage);
        NarrationExtractor(searchAllowed, somniumNarration);
        SayingExtractor(searchAllowed, somniumEventMessage);
        NarrationExtractor(searchAllowed, somniumEventNarration);
        SayingExtractor(searchAllowed, somniumFlashBackMessage);
        SayingExtractor(searchAllowed, somniumSubtitle);
        NarrationExtractor(searchAllowed, somniumLyrics);
        SomniumInteractOptions(searchAllowed);
    }
    

    public void SayingExtractor(bool allowSearch, DialogueSet target)
    {
        if (allowSearch && target.namePlate == null) {target.namePlate = GameObject.Find($"$Root/{target.canvas}/ScreenScaler/UIOff1/PanelNode/{target.location}/Rig/Name/Text")?.GetComponent<RawImage>();} // Necessary for finding the object
        if (allowSearch && target.dialogue == null) {target.dialogue = GameObject.Find($"$Root/{target.canvas}/ScreenScaler/UIOff1/PanelNode/{target.location}/Rig/Background/Text")?.GetComponent<TextMeshProUGUI>();} // Necessary for finding the object
        
        if (target.namePlate != null && target.dialogue != null)
        {
            string nameText = target.namePlate.mainTexture.name;
            string dialogueText = target.dialogue.text;

            if (!string.IsNullOrEmpty(dialogueText) && target.lastLine != dialogueText)
            {
                target.lastLine = dialogueText;
                SendBannerText($"{nameText} {target.verb}: {dialogueText}", false);
            }
        }
    }
    public void NarrationExtractor(bool allowSearch, TextOnlySet target)
    {
        if (allowSearch && target.message == null)  target.message = GameObject.Find($"$Root/{target.canvas}/ScreenScaler/UIOff1/PanelNode/{target.location}/GameObject/Background/Text")?.GetComponent<TextMeshProUGUI>();
        if (target.message != null)
        {
            string descrText = target.message.text;
            // ERROR Clicking description type windows with one line only, can cause the description not to be logged again. This is bad feedback and needs some kind of solution
            if (!string.IsNullOrEmpty(descrText) && (descrText != target.lastLine))
            {
                target.lastLine = descrText;
                SendBannerText($"Narration text: {descrText}", false);
            }
        }
    }
    
    public void InvestigationInteractOptions(bool allowSearch)
    {
        if (investigationInteraction.optionsObject == null)
        {
            if (!allowSearch) return;

            investigationInteraction.optionsObject = GameObject.Find("$Root/CommandCanvas/ScreenScaler/Command/Scale");
            if (investigationInteraction.optionsObject == null) return;
        }

        bool lookActive = investigationInteraction.optionsObject.transform.Find("Look").gameObject.activeSelf;

        if (investigationInteraction.interactionActive == lookActive) return; // no change
        investigationInteraction.interactionActive = lookActive;

        investigationInteraction.currentOptions.Clear();
        if (!lookActive)
        {
            OnLookDisable?.Invoke();
            investigationInteraction.focusTerm = "";
            return;
        }

        // update term
        string termText = investigationInteraction.optionsObject.transform.Find("Term/Background/Text")?.GetComponent<TextMeshProUGUI>().text;
        investigationInteraction.focusTerm = termText ?? "";
        ContextMessage cMsg = new ContextMessage($"Looking at {investigationInteraction.focusTerm}", false);
        OnTermChange?.Invoke(cMsg.ToJson());
        
        investigationInteraction.currentOptions["look_at_term"] = $"Look at {investigationInteraction.focusTerm}";

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
        OnLookChoicesUpdated?.Invoke(investigationInteraction.currentOptions);
    }

    public void SomniumInteractOptions(bool allowSearch)
    {
        if (somniumInteraction.optionsObject == null)
        {
            if (!allowSearch) return;

            somniumInteraction.optionsObject = GameObject.Find("$Root/3DUI");
            if (somniumInteraction.optionsObject == null) return;
        }
        

        bool anyActive = false;
        if (somniumInteraction.optionsObject.gameObject.activeSelf) // Only check buttons if the UI itself is active
        {
            for (int i = 0; i < 3; i++)
            {
                if (somniumInteraction.optionsObject.transform.GetChild(i).gameObject.activeSelf)
                {
                    anyActive = true;
                    break;
                }
            }
        }
        // If UI is off OR no buttons are active → no interaction
        if (!anyActive)
        {
            if (!somniumInteraction.interactionActive) return; // already inactive
            somniumInteraction.interactionActive = false;
            return;
        }
        // Only continue if state changed
        if (somniumInteraction.interactionActive == anyActive) return;
        somniumInteraction.interactionActive = anyActive;

        somniumInteraction.currentOptions.Clear();

        // Checks if button is and is active
        SomniumAddButtonIfActive("Command1", "button_up");
        SomniumAddButtonIfActive("Command3", "button_left");
        SomniumAddButtonIfActive("Command2", "button_right");

        // emit event
        OnLookChoicesUpdated?.Invoke(somniumInteraction.currentOptions);
    }


    // Helper functions
    List<string> moveable = new List<string>(){"zoom_button", "xray_button", "night_vision_button", "zoom_night_vision_button", "zoom_xray_button"};
    private void AddButtonIfActive(string buttonName, string KeyPrefix, string customText="")
    {
        string finalKey = KeyPrefix;
        var buttonObj = investigationInteraction.optionsObject.transform.Find(buttonName)?.gameObject; // Find button object
        if (buttonObj != null && buttonObj.activeSelf) // Make sure the button exists AND is active
        {
            var textComp = investigationInteraction.optionsObject.transform.Find(buttonName + "/Background/Text")?.GetComponent<TextMeshProUGUI>(); // Store the button GameObject
            if (moveable.Contains(KeyPrefix))
            {
                bool activeComp = investigationInteraction.optionsObject.transform.Find(buttonName + "/Button").gameObject.activeSelf;
                if (activeComp) finalKey += "_l";
                else finalKey += "_r";
            }
            if ( textComp != null) investigationInteraction.currentOptions[finalKey] = TextCleaner.Clean(textComp.text); // Use the button text as action description
            else investigationInteraction.currentOptions[finalKey] = customText; // Use custom text as action description
        }
    }
    private void SomniumAddButtonIfActive(string buttonName, string KeyPrefix)
    {
        var buttonObj = somniumInteraction.optionsObject.transform.Find(buttonName)?.gameObject; // Find button object
        if (buttonObj != null && buttonObj.activeSelf) // Make sure the button exists AND is active
        {
            var textComp = somniumInteraction.optionsObject.transform.Find(buttonName + "/Text")?.GetComponent<TextMeshPro>(); // Store the button GameObject
            if ( textComp != null) somniumInteraction.currentOptions[KeyPrefix] = textComp.text; // Use the button text as action description
        }
    }
    private void SendBannerText(string message, bool isSilent)
    {
        message = TextCleaner.Clean(message);

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

public class NetworkClient
{
    private WebSocket ws;
    private float retryTimer = 0f;
    private float retryDelay = 5f;
    private bool shouldRetry = false;
    public event Action<string> OnMessageReceived;
    public string web_url = ConfigLoader.LoadWebSocketURL();

    private Queue<string> sendQueue = new Queue<string>();
    private float sendTimer = 0f;
    private float sendDelay = 0.2f;

    public void Connect()
    {
        if (ws != null)
        {
            try { ws.Close(); }
            catch {}
        }
        ws = new WebSocket($"{web_url}");

        ws.OnOpen += OnOpen;
        ws.OnMessage += OnMessage;
        ws.OnError += OnError;
        ws.OnClose += OnClose;

        try
        {
            ws.Connect();
        }
        catch (Exception e)
        {
            Debug.Log("[WebSocket] Connect exception: " + e.Message);
            shouldRetry = true;
            retryTimer = 0f;
        }
    }

    public void Tick()
    {
        Reconnect();
        SendQueuedAction();
    }

    public void SendQueuedAction()
    {
        if (ws == null || !ws.IsAlive) return;

        sendTimer += Time.deltaTime;

        if (sendTimer < sendDelay) return;
        sendTimer = 0f;

        lock (sendQueue)
        {
            if (sendQueue.Count > 0)
            {
                string msg = sendQueue.Dequeue();
                try
                {
                    ws.Send(msg);
                }
                catch (Exception e)
                {
                    Debug.Log("[WebSocket] Send failed" + e.Message);
                }
            }
        }
    }

    public void Reconnect()
    {
        if (!shouldRetry) return;
        if (ws != null && ws.IsAlive) return;

        retryTimer += Time.deltaTime;

        if (retryTimer >= retryDelay)
        {
            web_url = ConfigLoader.LoadWebSocketURL();
            Debug.Log("[WebSocket] Retrying connection...");
            shouldRetry = false;
            Connect();
        }
    }

    private void OnOpen(object sender, System.EventArgs e)
    {
        Debug.Log("[WebSocket] Connected");
    }

    private void OnMessage(object sender, MessageEventArgs e)
    {
        Debug.Log("[WebSocket] Received: " + e.Data);
        string text = e.Data;
        OnMessageReceived?.Invoke(text);
    }

    private void OnError(object sender, WebSocketSharp.ErrorEventArgs e)
    {
        Debug.Log("[WebSocket] Error: " + e.Message);
        shouldRetry = true;
        retryTimer = 0f;
    }

    private void OnClose(object sender, CloseEventArgs e)
    {
        Debug.Log("[WebSocket] Closed: " + e.Code);
        shouldRetry = true;
        retryTimer = 0f;
    }

    public void SendString(string json)
    {
        lock ( sendQueue)
        {
            sendQueue.Enqueue(json);
        }
        //ws.Send(json);
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
            case "zoom_button_l":
                PressButton("BUTTON_LSTICK");
                break;
            case "thermo_button_l":
                PressButton("BUTTON_LSTICK");
                break;
            case "night_vision_button_l":
                PressButton("BUTTON_LSTICK");
                break;
            case "night_vision_button_r":
                PressButton("BUTTON_RSTICK");
                break;
            case "xray_button_l":
                PressButton("BUTTON_LSTICK");
                break;
            case "xray_button_r":
                PressButton("BUTTON_RSTICK");
                break;
            case "zoom_thermo_button_l":
                PressButton("BUTTON_LSTICK");
                break;
            case "zoom_xray_button_l":
                PressButton("BUTTON_LSTICK"); 
                break;
            case "zoom_xray_button_r":
                PressButton("BUTTON_RSTICK"); 
                break;
            case "zoom_night_vision_button_l":
                PressButton("BUTTON_LSTICK");
                break;
            case "zoom_night_vision_button_r":
                PressButton("BUTTON_RSTICK");
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
public static class JSON
{
    public static string ExtractJsonValue(string json, string key)
    {
        string pattern = $"\"{key}\":\"(.*?)\"";
        System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(json, pattern);
        if (match.Success) return match.Groups[1].Value;
        return "";
    }
}

public static class ConfigLoader
{
    public static string LoadWebSocketURL()
    {
        string path = Path.Combine(Paths.PluginPath, "config_websocket_url.txt");

        if (!File.Exists(path))
        {
            Debug.Log("[Config] File not found, using default");
            return "ws://localhost:8000";
        }

        string text = File.ReadAllText(path).Trim();

        if (string.IsNullOrEmpty(text))
        {
            Debug.Log("[Config] File empty, using default");
            return "ws://localhost:8000";
        }

        return text;
    }
}

// Resources
public static class TextCleaner
{
    static Dictionary<string, string> nameMap = new Dictionary<string, string>()
    {
        {"ui_main_name_c00", "Unknown"},
        {"ui_main_name_c01", "Kaname Date"},
        {"ui_main_name_c02", "Aiba"},
        {"ui_main_name_c04", "Hitomi Sagan"},
        {"ui_main_name_c05", "Iris Sagan"},
        {"ui_main_name_c06", "Ota Matsushita"},
        {"ui_main_name_c09", "Boss"},
        {"ui_main_name_c11", "Mayumi Matsushita"},
        {"ui_main_name_c51", "Inspector"},
        {"ui_main_name_c52", "Policeman"},
        {"ui_main_name_c102", "A-set"},
    };

    public static string Clean(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        string text = input;

        text = RemoveRichText(text);
        text = NormalizeWhitespace(text);
        text = ReplaceQuotes(text);
        text = ResolveCharacterNames(text);

        return text;
    }

    public static string ReplaceQuotes(string text)
    {
        return text.Replace("\"","\'");
    }
    public static string RemoveRichText(string text)
    {
        return System.Text.RegularExpressions.Regex.Replace(text, "<[^>]+>", string.Empty);
    }
    public static string ResolveCharacterNames(string text)
    {
        foreach (var kv in nameMap)
        {
            text = text.Replace(kv.Key, kv.Value);
        }
        return text;
    }
    public static string NormalizeWhitespace(string text)
    {
        return text.Replace("\r\n", " ")
               .Replace("\n", " ")
               .Replace("\r", " ");
    }
}

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