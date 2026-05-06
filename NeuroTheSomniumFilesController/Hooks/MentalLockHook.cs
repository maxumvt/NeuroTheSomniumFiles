namespace NeuroTheSomniumFiles;

using HarmonyLib;
using System;
using TMPro;
using UnityEngine;

[HarmonyPatch(typeof(TMP_Text), "set_text")]
public static class MentalLock_set_text_Patch
{
    private static string LastMentalLockClear = "";

    static void Postfix(TMP_Text __instance, string __0)
    {
        try {
            if ( !( __instance.transform.parent.name == "LockClear" && __instance.name == "Text[#*]") )
                return;
            
            if ( !( LastMentalLockClear == __0 ) )
            {
                LastMentalLockClear = __0;
                
                var task_cleared_message = __instance.transform.parent.Find("Text")?.GetComponent<TextMeshProUGUI>()?.text ?? "";
                string cleaned_task_cleared_message = TextCleaner.Clean(task_cleared_message);
                string cleaned_lock_text = TextCleaner.Clean(__0);
                ContextMessage.CreateContentMessage($"Mental lock {cleaned_lock_text} cleared: {task_cleared_message}", false);
            }
        }
        catch (Exception ex) {
            Debug.LogError("[DialogueHook] Exception: " + ex);
        }
    }
}