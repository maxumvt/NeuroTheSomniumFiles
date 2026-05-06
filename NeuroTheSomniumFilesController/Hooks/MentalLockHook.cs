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
            if ( !( __instance.transform.parent.name == "LockClear" ) )
                return;
            
            if ( !( LastMentalLockClear == __0 ) )
            {
                LastMentalLockClear = __0;

                string cleaned_lock_text = TextCleaner.Clean(__0);
                ContextMessage msg = new ContextMessage($"Mental lock: {cleaned_lock_text} cleared", false);
                NetworkClient.SendString(JSON.ToJson(msg.message));
            }

            Debug.Log($"This is LockClear working");

            // string title = __instance.transform.parent.parent.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().text;
            // string purpose = __instance.transform.parent.parent.GetChild(5).GetComponent<TextMeshProUGUI>().text;
            // string briefing = __0;

            // title = TextCleaner.Clean(title);
            // purpose = TextCleaner.Clean(purpose);
            // briefing = TextCleaner.Clean(briefing);

            // string formatted = $"Mission title: {title}, Mission purpose: {purpose}, Mission briefing: {briefing}";
            // ContextMessage cMSG = new ContextMessage(formatted, false);

            // NetworkClient.SendString(JSON.ToJson(cMSG.message));

        }
        catch (Exception ex) {
            Debug.LogError("[DialogueHook] Exception: " + ex);
        }
    }
}