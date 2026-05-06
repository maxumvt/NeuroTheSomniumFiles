namespace NeuroTheSomniumFiles;

using HarmonyLib;
using System;
using TMPro;
using UnityEngine;

[HarmonyPatch(typeof(TMP_Text), "set_text")]
public static class Timer_set_text_Patch
{
    static float LastTimeLeft = 300f;
    static float UpdateInterval = 10f;

    static void Postfix(TMP_Text __instance, string __0)
    {
        try {
            if (!(__instance.transform.parent.parent.name == "Time"))
                return;

            float timeFloat = TextCleaner.TimeConvert(__0);
            if ( LastTimeLeft - UpdateInterval >= timeFloat )
            {
                ContextMessage.CreateContentMessage($"Time on the clock is: {__0} second(s)", false);
                LastTimeLeft = timeFloat;
            }

        }
        catch (Exception ex) {
            Debug.LogError("[DialogueHook] Exception: " + ex);
        }
    }
}