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

public class AgentController
{
    private NetworkClient network;
    private ObservationProvider observations;
    private ActionRegistry actions;

    public AgentController(NetworkClient net,
                           ObservationProvider obs,
                           ActionRegistry act)
    {
        network = net;
        observations = obs;
        actions = act;
    }

    public void Initialize()
    {
        network.Connect();
        network.OnMessageReceived += actions.Validate;
    }

    public void Tick()
    {
        var obs = observations.Collect();
        //network.SendString(obs);
    }
}

public class ObservationProvider
{
    public ObservationData Collect()
    {
        return new ObservationData
        {
            target = "boss", entity = "boss", text= $"says: something"
        };
    }
}

public class ActionRegistry : BaseUnityPlugin
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
        Logger.LogInfo($"this is received in ActionRegistery: {json}");
    }

    public void Execute(string data)
    {
        
    }
}

public class NetworkClient : BaseUnityPlugin
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
        Logger.LogInfo("[WebSocket] Connected");
    }

    private void OnMessage(object sender, MessageEventArgs e)
    {
        Logger.LogInfo("[WebSocket] Received: " + e.Data);
        string text = e.Data;
        OnMessageReceived?.Invoke(text);
    }

    private void OnError(object sender, ErrorEventArgs e)
    {
        Logger.LogInfo("[WebSocket] Error: " + e.Message);
    }

    private void OnClose(object sender, CloseEventArgs e)
    {
        Logger.LogInfo("[WebSocket] Closed: " + e.Code);
    }

    public void SendString(string json)
    {
        if (ws != null && ws.IsAlive)
        {
            ws.Send(json);
        }
    }
}


public class ObservationData
{
    public string target;
    public string entity;
    public string text;
}


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