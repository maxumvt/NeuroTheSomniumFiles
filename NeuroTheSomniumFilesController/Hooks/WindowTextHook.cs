namespace NeuroTheSomniumFiles;

using HarmonyLib;
using System;
using UnityEngine;
using UnityEngine.UI;

[HarmonyPatch(typeof(Game.TextController), "SetNextLine")]
public static class TextController_SetNextLine_Patch
{
    static void Postfix(Game.TextController __instance, string __0)
    {
        try
        {
            JudgeLocation(__instance, __0);
        }
        catch (Exception ex)
        {
            Debug.LogError("[DialogueHook] Exception: " + ex);
        }
    }

    static void JudgeLocation(Game.TextController instance, string new_text)
    {
        var target = instance.transform.parent.parent.parent.ToString();
        //Debug.Log(target);

        if (target.Contains("MessageWindow"))
            DialogueWindow(instance, new_text);
        if (target.Contains("NarrationWindow"))
            NarrationWindow(instance, new_text);
        if (target.Contains("Explanation"))
            Explanation(instance, new_text);
        if (target.Contains("Subtitle"))
            Subtitle(instance, new_text);
        if (target.Contains("FlashBackWindow"))
            FlashBackWindow(instance, new_text);
        if (target.Contains("Lyrics"))
            Lyrics(instance, new_text);
    }

    static void DialogueWindow(Game.TextController instance, string new_text)
    {
        var speaker_code = instance.transform.parent.parent.GetChild(1).GetChild(0).GetComponent<RawImage>().mainTexture.name;
        var speaker = TextCleaner.ResolveCharacterNames(speaker_code);

        string cleaned = TextCleaner.Clean(new_text);
        string formatted = $"{speaker} says: \"{cleaned}\"";
        
        ContextMessage.CreateContentMessage(formatted, false);
    }
    static void NarrationWindow(Game.TextController _instance, string new_text)
    {
        string cleaned = TextCleaner.Clean(new_text);
        string formatted = $"Narrated: \"{cleaned}\"";
        ContextMessage.CreateContentMessage(formatted, false);
    }
    static void Explanation(Game.TextController _instance, string new_text)
    {
        string cleaned = TextCleaner.Clean(new_text);
        string formatted = $"Narrated: \"{cleaned}\"";
        ContextMessage.CreateContentMessage(formatted, false);
    }
    static void Subtitle(Game.TextController instance, string new_text)
    {
        var speaker_code = instance.transform.parent.parent.GetChild(1).GetChild(0).GetComponent<RawImage>().mainTexture.name;
        var speaker = TextCleaner.ResolveCharacterNames(speaker_code);
 
        string cleaned = TextCleaner.Clean(new_text);
        string formatted = $"{speaker} says: \"{cleaned}\"";
        ContextMessage.CreateContentMessage(formatted, false);
    }
    static void FlashBackWindow(Game.TextController instance, string new_text)
    {
        var speaker_code = instance.transform.parent.parent.GetChild(1).GetChild(0).GetComponent<RawImage>().mainTexture.name;
        var speaker = TextCleaner.ResolveCharacterNames(speaker_code);

 
        string cleaned = TextCleaner.Clean(new_text);
        string formatted = $"{speaker} said: \"{cleaned}\"";
        ContextMessage.CreateContentMessage(formatted, false);
    }
    static void Lyrics(Game.TextController _instance, string new_text)
    {
        string cleaned = TextCleaner.Clean(new_text);
        string formatted = $"Narrated: \"{cleaned}\"";
        ContextMessage.CreateContentMessage(formatted, false);
    }
}
