namespace NeuroTheSomniumFiles;

using HarmonyLib;
using System;
using UnityEngine;

[HarmonyPatch(typeof(Game.TextController), "SetNextLine")]
public static class TextController_SetNextLine_Patch
{
    private static string lastLine;

    static void Postfix(Game.TextController __instance, string __0)
    {
        try
        {
            string text = __0;

            if (string.IsNullOrEmpty(text))
                return;

            // Prevent duplicates
            if (text == lastLine)
                return;

            lastLine = text;

            // You can enhance this later with speaker detection
            string cleaned = TextCleaner.Clean(text);
            string formatted = $"Unknown says: {cleaned}";
            ContextMessage cMSG = new ContextMessage(formatted, false);

            NetworkClient.SendString(JSON.ToJson(cMSG));
        }
        catch (Exception ex)
        {
            Debug.LogError("[DialogueHook] Exception: " + ex);
        }
    }
}
// static void Postfix(UnityEngine.UI.RawImage __instance, UnityEngine.Texture __0)
// {
//     try {
//        StringBuilder sb = new StringBuilder();
//        sb.AppendLine("--------------------");
//        sb.AppendLine("void UnityEngine.UI.RawImage::set_texture(UnityEngine.Texture value)");
//        sb.Append("- __instance: ").AppendLine(__instance.ToString());
//        sb.Append("- Parameter 0 'value': ").AppendLine(__0?.name.ToString() ?? "null");
//        if (__0?.name.ToString == "")
//             {
//                 return;
//             }
//        UnityExplorer.ExplorerCore.Log(sb.ToString());
//     }
//     catch (System.Exception ex) {
//         UnityExplorer.ExplorerCore.LogWarning($"Exception in patch of void UnityEngine.UI.RawImage::set_texture(UnityEngine.Texture value):\n{ex}");
//     }
// }
