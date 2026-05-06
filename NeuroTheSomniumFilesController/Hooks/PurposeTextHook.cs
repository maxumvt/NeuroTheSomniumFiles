namespace NeuroTheSomniumFiles;

using HarmonyLib;
using System;
using TMPro;
using UnityEngine;

[HarmonyPatch(typeof(TMP_Text), "set_text")]
public static class Purpose_set_text_Patch
{
    static void Postfix(TMP_Text __instance, string __0)
    {
        try {
            if (!(__instance.name=="BriefingText"))
                return;

            string title = __instance.transform.parent.parent.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().text;
            string purpose = __instance.transform.parent.parent.GetChild(5).GetComponent<TextMeshProUGUI>().text;
            string briefing = __0;

            title = TextCleaner.Clean(title);
            purpose = TextCleaner.Clean(purpose);
            briefing = TextCleaner.Clean(briefing);

            string formatted = $"Mission title: {title}, Mission purpose: {purpose}, Mission briefing: {briefing}";
            ContextMessage.CreateContentMessage(formatted, false);
        }
        catch (Exception ex) {
            Debug.LogError("[DialogueHook] Exception: " + ex);
        }
    }
}