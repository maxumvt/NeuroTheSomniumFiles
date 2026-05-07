namespace NeuroTheSomniumFiles;

using HarmonyLib;
using System;
using TMPro;
using UnityEngine;

[HarmonyPatch(typeof(TMP_Text), "set_text")]
public static class TaskObserver_set_text_Patch
{
    private static string current_task = "";
    private static string primary_task = "";
    private static string secondary_task = "";

    static void Postfix(TMP_Text __instance, string __0)
    {
        try {
            if ( !( __instance.transform.parent.parent.parent.name == "PurposeRoot" ) )
                return;
            
            if (__0.Contains("A:"))
            {
                if (primary_task == __0)
                    return;

                primary_task = __0;
                string cleaned_task = TextCleaner.Clean(__0);
                ContextMessage.CreateContentMessage($"The first branch task is: \"{cleaned_task}\"", false);
                return;
            }
            if (__0.Contains("B:"))
            {
                if (secondary_task == __0)
                    return;
                
                secondary_task = __0;
                string cleaned_task = TextCleaner.Clean(__0);
                ContextMessage.CreateContentMessage($"The second branch task is: \"{cleaned_task}\"", false);
                return;
            }

            if ( !( current_task == __0 ) )
            {
                current_task = __0;
                string cleaned_task = TextCleaner.Clean(__0);
                ContextMessage.CreateContentMessage($"The current task is: \"{cleaned_task}\"", false);
            }
        }
        catch (Exception ex) {
            Debug.LogError("[DialogueHook] Exception: " + ex);
        }
    }
}