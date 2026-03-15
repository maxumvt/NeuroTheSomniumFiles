namespace NeuroSomniumFilesController;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

using WebSocketSharp;
using BepInEx;
using System.Collections.Generic;
using System;


[BepInPlugin("com.maxum.dialoglogger", "NeuroSomniumFiles", "1.0.0")]
public class MyPlugin : BaseUnityPlugin
{
    private AgentController agent;

    void Awake()
    {
        var network = new NetworkClient();
        var actions = new ActionRegistry();
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
        network.Connect();
        NeuroMessage startUpMsg = new NeuroMessage();
        network.SendString(startUpMsg.StartupToJson());

        network.OnMessageReceived += actions.Validate;
        observations.OnBannerText += network.SendString;
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
    public event Action<string> OnBannerText;

    public RawImage characterNamePlate;
    public TextMeshProUGUI characterDialogue;
    public TextMeshProUGUI descriptionDialogue;
    public string dialogueLastline;
    public string descriptionLastline;

    public void Collect(bool searchAllowed)
    {
        CharacterSpeaking(searchAllowed);
        DescriptionText(searchAllowed);
    }

    void CharacterSpeaking(bool allowSearch)
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
    void DescriptionText(bool allowSearch)
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
}

public class ActionRegistry
{
    private Dictionary<string, string> actions
        = new Dictionary<string, string>();

    public void Register(string name, string action)
    {
        actions[name] = action;
    }

    public void Unregister(string name)
    {
        actions.Remove(name);
    }

    public void Validate(string json)
    {
        Debug.Log($"this is received in ActionRegistery: {json}");
    }

    public void Execute(string data)
    {
        
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
            + "\"game\":\"" + game + "\","
            + "}";
    }
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