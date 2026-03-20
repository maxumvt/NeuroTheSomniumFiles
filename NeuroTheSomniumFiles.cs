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

    // Condition trackers Investigation
    // Normal:
    public RawImage messageNamePlate;
    public TextMeshProUGUI messageDialogue;
    public string dialogueLastLine;
    public TextMeshProUGUI narrationDialogue;
    public string narrationLastLine;
    // Event:
    public RawImage eventMessageNamePlate;
    public TextMeshProUGUI eventMessageDialogue;
    public string eventDialogueLastLine;
    public TextMeshProUGUI eventNarrationDialogue;
    public string eventNarrationLastLine;
    // Flash Back:
    public RawImage flashBackMessageNamePlate;
    public TextMeshProUGUI flashBackMessageDialogue;
    public string flashBackDialogueLastLine;
    // Subtitle:
    public RawImage subtitleNamePlate;
    public TextMeshProUGUI subtitleDialogue;
    public string subtitleLastLine;
    // Lyrics:
    public TextMeshProUGUI lyricsDialogue;
    public string lyricsLastLine;

    // Condition trackers Somnium
    // Normal:
    public RawImage somniumMessageNamePlate;
    public TextMeshProUGUI somniumMessageDialogue;
    public string somniumDialogueLastLine;
    public TextMeshProUGUI somniumNarrationDialogue;
    public string somniumNarrationLastLine;
    // Event:
    public RawImage somniumEventMessageNamePlate;
    public TextMeshProUGUI somniumEventMessageDialogue;
    public string somniumEventDialogueLastLine;
    public TextMeshProUGUI somniumEventNarrationDialogue;
    public string somniumEventNarrationLastLine;
    // Flash Back:
    public RawImage somniumFlashBackMessageNamePlate;
    public TextMeshProUGUI somniumFlashBackMessageDialogue;
    public string somniumFlashBackDialogueLastLine;
    // Subtitle:
    public RawImage somniumSubtitleNamePlate;
    public TextMeshProUGUI somniumSubtitleDialogue;
    public string somniumSubtitleLastLine;
    // Lyrics:
    public TextMeshProUGUI somniumLyricsDialogue;
    public string somniumLyricsLastLine;

    // Interact options Investigation:
    public GameObject lookChoices;
    public bool interactLook = false;
    public string currentTerm = "";
    public Dictionary<string, string> CurrentOptions = new Dictionary<string, string>();
    // Interaction options Somnium
    public GameObject SomniumInteractOptionsObject;
    public bool somniumInteractLook = false;
    public bool somniumInteract = false;
    public Dictionary<string, string> SomniumCurrentOptions = new Dictionary<string, string>();


    public void Collect(bool searchAllowed)
    {
        MessageWindow(searchAllowed);
        NarrationWindow(searchAllowed);
        EventMessageWindow(searchAllowed);
        EventNarrationWindow(searchAllowed);
        FlashBackWindow(searchAllowed);
        SubtitleWindow(searchAllowed);
        Lyrics(searchAllowed);
        InvestigationInteractOptions(searchAllowed);

        SomniumMessageWindow(searchAllowed);
        SomniumNarrationWindow(searchAllowed);
        SomniumEventMessageWindow(searchAllowed);
        //SomniumFlashBackWindow(searchAllowed);
        SomniumEventNarrationWindow(searchAllowed);
        SomniumSubtitleWindow(searchAllowed);
        SomniumLyrics(searchAllowed);
        SomniumInteractOptions(searchAllowed);

    }
    
    // Investigation
    public void MessageWindow(bool allowSearch)
    {
        if (allowSearch && messageNamePlate == null) {messageNamePlate = GameObject.Find("$Root/UICanvas/ScreenScaler/UIOff1/PanelNode/MessageWindow/Rig/Name/Text")?.GetComponent<RawImage>();} // Necessary for finding the object
        if (allowSearch && messageDialogue == null) {messageDialogue = GameObject.Find("$Root/UICanvas/ScreenScaler/UIOff1/PanelNode/MessageWindow/Rig/Background/Text")?.GetComponent<TextMeshProUGUI>();} // Necessary for finding the object
        
        if (messageNamePlate != null && messageDialogue != null)
        {
            string nameText = messageNamePlate.mainTexture.name;
            string dialogueText = messageDialogue.text;

            if (!string.IsNullOrEmpty(dialogueText) && dialogueLastLine != dialogueText)
            {
                dialogueLastLine = dialogueText;
                SendBannerText($"{nameText} says: {dialogueText}", false);
            }
        }
    }
    public void NarrationWindow(bool allowSearch)
    {
        if (allowSearch && narrationDialogue == null)  narrationDialogue = GameObject.Find("$Root/UICanvas/ScreenScaler/UIOff1/PanelNode/NarrationWindow/GameObject/Background/Text")?.GetComponent<TextMeshProUGUI>();
        if (narrationDialogue != null)
        {
            string descrText = narrationDialogue.text;
            // ERROR Clicking description type windows with one line only, can cause the description not to be logged again. This is bad feedback and needs some kind of solution
            if (!string.IsNullOrEmpty(descrText) && (descrText != narrationLastLine))
            {
                narrationLastLine = descrText;
                SendBannerText($"Narration text: {descrText}", false);
            }
        }
    }
    public void EventMessageWindow(bool allowSearch)
    {
        if (allowSearch && eventMessageNamePlate == null) {eventMessageNamePlate = GameObject.Find("$Root/UICanvas/ScreenScaler/UIOff1/PanelNode/EventMessageWindow/Rig/Name/Text")?.GetComponent<RawImage>();} // Necessary for finding the object
        if (allowSearch && eventMessageDialogue == null) {eventMessageDialogue = GameObject.Find("$Root/UICanvas/ScreenScaler/UIOff1/PanelNode/EventMessageWindow/Rig/Background/Text")?.GetComponent<TextMeshProUGUI>();} // Necessary for finding the object
        
        if (eventMessageNamePlate != null && eventMessageDialogue != null)
        {
            string nameText = eventMessageNamePlate.mainTexture.name;
            string dialogueText = eventMessageDialogue.text;

            if (!string.IsNullOrEmpty(dialogueText) && eventDialogueLastLine != dialogueText)
            {
                eventDialogueLastLine = dialogueText;
                SendBannerText($"{nameText} says: {dialogueText}", false);
            }
        }
    }
    public void EventNarrationWindow(bool allowSearch)
    {
        if (allowSearch && eventNarrationDialogue == null)  eventNarrationDialogue = GameObject.Find("$Root/UICanvas/ScreenScaler/UIOff1/PanelNode/EventNarrationWindow/GameObject/Background/Text")?.GetComponent<TextMeshProUGUI>();
        if (eventNarrationDialogue != null)
        {
            string descrText = eventNarrationDialogue.text;
            // ERROR Clicking description type windows with one line only, can cause the description not to be logged again. This is bad feedback and needs some kind of solution
            if (!string.IsNullOrEmpty(descrText) && (descrText != eventNarrationLastLine))
            {
                eventNarrationLastLine = descrText;
                SendBannerText($"Narration text: {descrText}", false);
            }
        }
    }
    public void FlashBackWindow(bool allowSearch)
    {
        if (allowSearch && flashBackMessageNamePlate == null) {flashBackMessageNamePlate = GameObject.Find("$Root/UICanvas/ScreenScaler/UIOff1/PanelNode/FlashBackWindow/Rig/Name/Text")?.GetComponent<RawImage>();} // Necessary for finding the object
        if (allowSearch && flashBackMessageDialogue == null) {flashBackMessageDialogue = GameObject.Find("$Root/UICanvas/ScreenScaler/UIOff1/PanelNode/FlashBackWindow/Rig/Background/Text")?.GetComponent<TextMeshProUGUI>();} // Necessary for finding the object
        
        if (flashBackMessageNamePlate != null && flashBackMessageDialogue != null)
        {
            string nameText = flashBackMessageNamePlate.mainTexture.name;
            string dialogueText = flashBackMessageDialogue.text;

            if (!string.IsNullOrEmpty(dialogueText) && flashBackDialogueLastLine != dialogueText)
            {
                flashBackDialogueLastLine = dialogueText;
                SendBannerText($"In a flashback {nameText} said: {dialogueText}", false);
            }
        }
    }
    public void SubtitleWindow(bool allowSearch)
    {
        if (allowSearch && subtitleNamePlate == null) {subtitleNamePlate = GameObject.Find("$Root/UICanvas/ScreenScaler/UIOff1/PanelNode/SubtitleWindow/Rig/Name/Text")?.GetComponent<RawImage>();} // Necessary for finding the object
        if (allowSearch && subtitleDialogue == null) {subtitleDialogue = GameObject.Find("$Root/UICanvas/ScreenScaler/UIOff1/PanelNode/SubtitleWindow/Rig/Background/Text")?.GetComponent<TextMeshProUGUI>();} // Necessary for finding the object
        
        if (subtitleNamePlate != null && subtitleDialogue != null)
        {
            string nameText = subtitleNamePlate.mainTexture.name;
            string dialogueText = subtitleDialogue.text;

            if (!string.IsNullOrEmpty(dialogueText) && subtitleLastLine != dialogueText)
            {
                subtitleLastLine = dialogueText;
                SendBannerText($"{nameText} says: {dialogueText}", false);
            }
        }
    }
    public void Lyrics(bool allowSearch)
    {
        if (allowSearch && lyricsDialogue == null)  lyricsDialogue = GameObject.Find("$Root/UICanvas/ScreenScaler/UIOff1/PanelNode/Lyrics/Back/Text")?.GetComponent<TextMeshProUGUI>();
        if (lyricsDialogue != null)
        {
            string descrText = lyricsDialogue.text;
            // ERROR Clicking description type windows with one line only, can cause the description not to be logged again. This is bad feedback and needs some kind of solution
            if (!string.IsNullOrEmpty(descrText) && (descrText != lyricsLastLine))
            {
                lyricsLastLine = descrText;
                SendBannerText($"Lyrics text: {descrText}", false);
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

    // Somnium
    public void SomniumMessageWindow(bool allowSearch)
    {
        if (allowSearch && somniumMessageNamePlate == null) {somniumMessageNamePlate = GameObject.Find("$Root/Canvas (1)/ScreenScaler/UIOff1/PanelNode/MessageWindow/Rig/Name/Text")?.GetComponent<RawImage>();} // Necessary for finding the object
        if (allowSearch && somniumMessageDialogue == null) {somniumMessageDialogue = GameObject.Find("$Root/Canvas (1)/ScreenScaler/UIOff1/PanelNode/MessageWindow/Rig/Background/Text")?.GetComponent<TextMeshProUGUI>();} // Necessary for finding the object
        
        if (somniumMessageNamePlate != null && somniumMessageDialogue != null)
        {
            string nameText = somniumMessageNamePlate.mainTexture.name;
            string dialogueText = somniumMessageDialogue.text;

            if (!string.IsNullOrEmpty(dialogueText) && somniumDialogueLastLine != dialogueText)
            {
                somniumDialogueLastLine = dialogueText;
                SendBannerText($"{nameText} says: {dialogueText}", false);
            }
        }
    }
    public void SomniumNarrationWindow(bool allowSearch)
    {
        if (allowSearch && somniumNarrationDialogue == null)  somniumNarrationDialogue = GameObject.Find("$Root/Canvas (1)/ScreenScaler/UIOff1/PanelNode/NarrationWindow/GameObject/Background/Text")?.GetComponent<TextMeshProUGUI>();
        if (somniumNarrationDialogue != null)
        {
            string descrText = somniumNarrationDialogue.text;
            // ERROR Clicking description type windows with one line only, can cause the description not to be logged again. This is bad feedback and needs some kind of solution
            if (!string.IsNullOrEmpty(descrText) && (descrText != somniumNarrationLastLine))
            {
                somniumNarrationLastLine = descrText;
                SendBannerText($"Narration text: {descrText}", false);
            }
        }
    }
    public void SomniumEventMessageWindow(bool allowSearch)
    {
        if (allowSearch && somniumEventMessageNamePlate == null) {somniumEventMessageNamePlate = GameObject.Find("$Root/Canvas (1)/ScreenScaler/UIOff1/PanelNode/EventMessageWindow/Rig/Name/Text")?.GetComponent<RawImage>();} // Necessary for finding the object
        if (allowSearch && somniumEventMessageDialogue == null) {somniumEventMessageDialogue = GameObject.Find("$Root/Canvas (1)/ScreenScaler/UIOff1/PanelNode/EventMessageWindow/Rig/Background/Text")?.GetComponent<TextMeshProUGUI>();} // Necessary for finding the object
        
        if (somniumEventMessageNamePlate != null && somniumEventMessageDialogue != null)
        {
            string nameText = somniumEventMessageNamePlate.mainTexture.name;
            string dialogueText = somniumEventMessageDialogue.text;

            if (!string.IsNullOrEmpty(dialogueText) && somniumEventDialogueLastLine != dialogueText)
            {
                somniumEventDialogueLastLine = dialogueText;
                SendBannerText($"{nameText} says: {dialogueText}", false);
            }
        }
    }
    public void SomniumEventNarrationWindow(bool allowSearch)
    {
        if (allowSearch && somniumEventNarrationDialogue == null)  somniumEventNarrationDialogue = GameObject.Find("$Root/Canvas (1)/ScreenScaler/UIOff1/PanelNode/EventNarrationWindow/GameObject/Background/Text")?.GetComponent<TextMeshProUGUI>();
        if (somniumEventNarrationDialogue != null)
        {
            string descrText = somniumEventNarrationDialogue.text;
            // ERROR Clicking description type windows with one line only, can cause the description not to be logged again. This is bad feedback and needs some kind of solution
            if (!string.IsNullOrEmpty(descrText) && (descrText != somniumEventNarrationLastLine))
            {
                somniumEventNarrationLastLine = descrText;
                SendBannerText($"Narration text: {descrText}", false);
            }
        }
    }
    public void SomniumFlashBackWindow(bool allowSearch)
    {
        if (allowSearch && somniumFlashBackMessageNamePlate == null) {somniumFlashBackMessageNamePlate = GameObject.Find("$Root/Canvas (1)/ScreenScaler/UIOff1/PanelNode/FlashBackWindow/Rig/Name/Text")?.GetComponent<RawImage>();} // Necessary for finding the object
        if (allowSearch && somniumFlashBackMessageDialogue == null) {somniumFlashBackMessageDialogue = GameObject.Find("$Root/Canvas (1)/ScreenScaler/UIOff1/PanelNode/FlashBackWindow/Rig/Background/Text")?.GetComponent<TextMeshProUGUI>();} // Necessary for finding the object
        
        if (somniumFlashBackMessageNamePlate != null && somniumFlashBackMessageDialogue != null)
        {
            string nameText = somniumFlashBackMessageNamePlate.mainTexture.name;
            string dialogueText = somniumFlashBackMessageDialogue.text;

            if (!string.IsNullOrEmpty(dialogueText) && somniumFlashBackDialogueLastLine != dialogueText)
            {
                somniumFlashBackDialogueLastLine = dialogueText;
                SendBannerText($"In a flashback {nameText} said: {dialogueText}", false);
            }
        }
    }
    public void SomniumSubtitleWindow(bool allowSearch)
    {
        if (allowSearch && somniumSubtitleNamePlate == null) {somniumSubtitleNamePlate = GameObject.Find("$Root/Canvas (1)/ScreenScaler/UIOff1/PanelNode/SubtitleWindow/Rig/Name/Text")?.GetComponent<RawImage>();} // Necessary for finding the object
        if (allowSearch && somniumSubtitleDialogue == null) {somniumSubtitleDialogue = GameObject.Find("$Root/Canvas (1)/ScreenScaler/UIOff1/PanelNode/SubtitleWindow/Rig/Background/Text")?.GetComponent<TextMeshProUGUI>();} // Necessary for finding the object
        
        if (somniumSubtitleNamePlate != null && somniumSubtitleDialogue != null)
        {
            string nameText = somniumSubtitleNamePlate.mainTexture.name;
            string dialogueText = somniumSubtitleDialogue.text;

            if (!string.IsNullOrEmpty(dialogueText) && somniumSubtitleLastLine != dialogueText)
            {
                somniumSubtitleLastLine = dialogueText;
                SendBannerText($"{nameText} says: {dialogueText}", false);
            }
        }
    }
    public void SomniumLyrics(bool allowSearch)
    {
        if (allowSearch && somniumLyricsDialogue == null)  somniumLyricsDialogue = GameObject.Find("$Root/Canvas (1)/ScreenScaler/UIOff1/PanelNode/Lyrics/Back/Text")?.GetComponent<TextMeshProUGUI>();
        if (somniumLyricsDialogue != null)
        {
            string descrText = somniumLyricsDialogue.text;
            // ERROR Clicking description type windows with one line only, can cause the description not to be logged again. This is bad feedback and needs some kind of solution
            if (!string.IsNullOrEmpty(descrText) && (descrText != somniumLyricsLastLine))
            {
                somniumLyricsLastLine = descrText;
                SendBannerText($"Lyrics text: {descrText}", false);
            }
        }
    }
    public void SomniumInteractOptions(bool allowSearch)
    {
        if (SomniumInteractOptionsObject == null)
        {
            if (!allowSearch) return;

            SomniumInteractOptionsObject = GameObject.Find("$Root/3DUI");
            if (SomniumInteractOptionsObject == null) return;
        }
        

        bool anyActive = false;
        if (SomniumInteractOptionsObject.gameObject.activeSelf) // Only check buttons if the UI itself is active
        {
            for (int i = 0; i < 3; i++)
            {
                if (SomniumInteractOptionsObject.transform.GetChild(i).gameObject.activeSelf)
                {
                    anyActive = true;
                    break;
                }
            }
        }
        // If UI is off OR no buttons are active → no interaction
        if (!anyActive)
        {
            if (!somniumInteractLook) return; // already inactive
            somniumInteractLook = false;
            return;
        }
        // Only continue if state changed
        if (somniumInteractLook == anyActive) return;
        somniumInteractLook = anyActive;

        SomniumCurrentOptions.Clear();

        // Checks if button is and is active
        SomniumAddButtonIfActive("Command1", "button_up");
        SomniumAddButtonIfActive("Command3", "button_left");
        SomniumAddButtonIfActive("Command2", "button_right");

        // emit event
        OnLookChoicesUpdated?.Invoke(SomniumCurrentOptions);
    }


    // Helper functions
    List<string> moveable = new List<string>(){"zoom_button", "xray_button", "night_vision_button", "zoom_night_vision_button", "zoom_xray_button"};
    private void AddButtonIfActive(string buttonName, string KeyPrefix, string customText="")
    {
        string finalKey = KeyPrefix;
        var buttonObj = lookChoices.transform.Find(buttonName)?.gameObject; // Find button object
        if (buttonObj != null && buttonObj.activeSelf) // Make sure the button exists AND is active
        {
            var textComp = lookChoices.transform.Find(buttonName + "/Background/Text")?.GetComponent<TextMeshProUGUI>(); // Store the button GameObject
            if (moveable.Contains(KeyPrefix))
            {
                bool activeComp = lookChoices.transform.Find(buttonName + "/Button").gameObject.activeSelf;
                if (activeComp) finalKey += "_l";
                else finalKey += "_r";
            }
            if ( textComp != null) CurrentOptions[finalKey] = TextCleaner.Clean(textComp.text); // Use the button text as action description
            else CurrentOptions[finalKey] = customText; // Use custom text as action description
        }
    }
    private void SomniumAddButtonIfActive(string buttonName, string KeyPrefix)
    {
        var buttonObj = SomniumInteractOptionsObject.transform.Find(buttonName)?.gameObject; // Find button object
        if (buttonObj != null && buttonObj.activeSelf) // Make sure the button exists AND is active
        {
            var textComp = SomniumInteractOptionsObject.transform.Find(buttonName + "/Text")?.GetComponent<TextMeshPro>(); // Store the button GameObject
            if ( textComp != null) SomniumCurrentOptions[KeyPrefix] = textComp.text; // Use the button text as action description
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
        // try
        // {
        //     System.IO.File.AppendAllText("D:\\temp\\ws_log.txt", e.Data + "\n");
        // }
        // catch {}

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
            System.IO.File.AppendAllText("D:\\temp\\ws_log.txt", json + "\n");
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

        text = NormalizeWhitespace(text);
        text = ReplaceQuotes(text);
        text = RemoveRichText(text);
        text = ResolveCharacterNames(text);

        return text;
    }
    public static string ReplaceQuotes(string text)
    {
        return text.Replace("\"","\'");
    }
    public static string RemoveRichText(string text)
    {
        return System.Text.RegularExpressions.Regex.Replace(text, "<.*?>", string.Empty);
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