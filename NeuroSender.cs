using WebSocketSharp;
using UnityEngine;

public class NeuroSender
{
    private WebSocket ws;

    public void Connect()
    {
        ws = new WebSocket("ws://localhost:8000");

        ws.OnOpen += OnOpen;
        ws.OnMessage += OnMessage;
        ws.OnError += OnError;
        ws.OnClose += OnClose;

        ws.Connect();
    }

    public void SendString(string json)
    {
        if (ws != null && ws.IsAlive)
        {
            ws.Send(json);
        }
    }

    private void OnOpen(object sender, System.EventArgs e)
    {
        Debug.Log("[WebSocket] Connected");
    }

    private void OnMessage(object sender, MessageEventArgs e)
    {
        Debug.Log("[WebSocket] Received: " + e.Data);
    }

    private void OnError(object sender, WebSocketSharp.ErrorEventArgs e)
    {
        Debug.Log("[WebSocket] Error: " + e.Message);
    }

    private void OnClose(object sender, CloseEventArgs e)
    {
        Debug.Log("[WebSocket] Closed: " + e.Code);
    }
}