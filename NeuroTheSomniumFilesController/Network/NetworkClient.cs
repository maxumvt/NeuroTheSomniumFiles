namespace NeuroTheSomniumFiles;

using System;
using System.Collections.Generic;
using WebSocketSharp;
using UnityEngine;

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
    }
}
