namespace NeuroTheSomniumFiles;

using HarmonyLib;
using System;
using TMPro;
using UnityEngine;

[HarmonyPatch(typeof(TMP_Text), "set_text")]
public static class Timer_set_text_Patch
{
    static float LastTimeLeft = 360f;
    static float UpdateInterval = 5f;

    static void Postfix(TMP_Text __instance, string __0)
    {
        try {
            if (!(__instance.name == "Time" && __instance.transform.parent.parent.name == "Timer"))
                return;

            float timeFloat = TextCleaner.TimeConvert(__0);
            if ( LastTimeLeft - UpdateInterval >= timeFloat && LastTimeLeft >= 0f)
            {
                ContextMessage.CreateContentMessage($"Time remaining: {timeFloat} second(s)", false);
                LastTimeLeft = timeFloat;
            }

        }
        catch (Exception ex) {
            Debug.LogError("[DialogueHook] Exception: " + ex);
        }
    }
}