namespace NeuroTheSomniumFiles;

using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[HarmonyPatch(typeof(GameObject), "SetActive")]
public static class SomniumDialogue_SetActive_Patch
{
    private static bool commands_parent_active = false;
    private static bool processing = false;

    private static List<BaseAction> previous_options = new();

    static void Postfix(GameObject __instance, bool __0)
    {
        try {
            if (! ( (__instance.name=="3DUI") || (__instance.name=="Command1") || (__instance.name=="Command2") || (__instance.name=="Command3") ) ) // Change to the correct name
                return;
            
            if (__instance.name == "3DUI")
            {
                commands_parent_active = __0; // Set the parent object to its state
                if (!__0 && previous_options.Count != 0)
                    ResetOptions();
                
                return;
            }
            
            if (processing) // Return when a command has been found active but another command has already been found active
                return;

            if (commands_parent_active && __0)
            {
                processing = true;
                CoroutineRunner.Run(CollectNextFrame(__instance));
            }

        }
        catch (Exception ex) {
            Debug.LogError("[DialogueHook] Exception: " + ex);
        }
    }

    private static IEnumerator CollectNextFrame(GameObject instance)
    {
        yield return null; // wait 1 frame

        List<BaseAction> options = new();

        var root = instance.transform.parent; // This is "3DUI"

        AddButton(root, options, "Command1", "button_up");
        AddButton(root, options, "Command2", "button_left");
        AddButton(root, options, "Command3", "button_right");

        if (options.Count == 0)
        {
            processing = false;
            yield break;
        }

        ActionRegisterMessage.CreateRegisterMessage(options);
        ActionforceMessage.CreateForceMessage(options);
        previous_options = options;
    }

    private static List<BaseAction> AddButton(Transform root, List<BaseAction> options, string buttonName, string key, string customText="")
    {
        var buttonObj = root.Find(buttonName)?.gameObject; // Find button object
        if (buttonObj == null || !buttonObj.activeSelf)
            return options;
        
        string text = root.transform.Find(buttonName + "/Text")?.GetComponent<TextMeshPro>().text;
        BaseAction newAction = new BaseAction(key, TextCleaner.Clean(text));
        options.Add(newAction);
        return options;
    }

    public static List<BaseAction> GetPreviousActions() { return previous_options; }
    public static void ResetOptions()
    {
        // Send unregister signal
        ActionUnregisterMessage.CreateUnregisterMessage(previous_options);
        processing = false;
        previous_options.Clear();
    }
}
