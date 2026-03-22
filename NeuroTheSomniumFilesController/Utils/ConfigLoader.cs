namespace NeuroTheSomniumFiles;

using System.IO;
using BepInEx;
using UnityEngine;

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
