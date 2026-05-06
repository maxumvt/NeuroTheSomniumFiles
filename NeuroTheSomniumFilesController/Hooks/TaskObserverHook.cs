namespace NeuroTheSomniumFiles;

using HarmonyLib;
using System;
using TMPro;
using UnityEngine;

[HarmonyPatch(typeof(TMP_Text), "set_text")]
public static class TaskObserver_set_text_Patch
{
    private static string current_task = "";

    static void Postfix(TMP_Text __instance, string __0)
    {
        try {
            if ( !( __instance.transform.parent.parent.parent.name == "PurposeRoot" ) )
                return;
            
            if ( !( current_task == __0 ) )
            {
                current_task = __0;
                
                string cleaned_task = TextCleaner.Clean(__0);
                ContextMessage msg = new ContextMessage($"The current task is: \"{cleaned_task}\"", false);
                NetworkClient.SendString(JSON.ToJson(msg.message));
            }
        }
        catch (Exception ex) {
            Debug.LogError("[DialogueHook] Exception: " + ex);
        }
    }
}