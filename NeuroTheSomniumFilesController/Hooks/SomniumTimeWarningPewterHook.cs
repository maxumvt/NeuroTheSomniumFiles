namespace NeuroTheSomniumFiles;

using HarmonyLib;
using System;
using TMPro;
using UnityEngine;

[HarmonyPatch(typeof(TMP_Text), "set_text")]
public static class TimeWarningPewter_set_text_Patch
{
    static string lastWarning = "";

    static void Postfix(TMP_Text __instance, string __0)
    {
        try {
            if (!(__instance.name == "Text" && __instance.transform.parent.parent.name == "PewterWindow"))
                return;

            if ( !(lastWarning == __0))
            {
                string cleaned_text = TextCleaner.Clean(__0);
                ContextMessage.CreateContentMessage($"Pewter says: {cleaned_text}", false);
                lastWarning = __0;
            }

        }
        catch (Exception ex) {
            Debug.LogError("[DialogueHook] Exception: " + ex);
        }
    }
}