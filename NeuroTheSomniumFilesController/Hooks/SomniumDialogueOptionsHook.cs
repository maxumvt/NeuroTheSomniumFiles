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

        GameObject timiesContainer = GameObject.Find($"$Root/MiddletCanvas/ScreenScaler/ItemWindow/Mask/Contents");
        int timies = timiesContainer.transform.childCount;
        for (var i = 0; i < timies; i++)
        {
            // get timie things
            string timieValue = timiesContainer.transform.GetChild(i).GetChild(2).GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text; // timies - 1 - 
            string timiePosition = (timies - i - 1).ToString();

            // add timie value to AddButton *3
            AddButtonWithTimie(root, options, "Command1", "button_up", timiePosition, timieValue);
            AddButtonWithTimie(root, options, "Command2", "button_right", timiePosition, timieValue);
            AddButtonWithTimie(root, options, "Command3", "button_left", timiePosition, timieValue);

        }
        // These ones will be in a loop for every time item that there is.
        AddButton(root, options, "Command1", "button_up");
        AddButton(root, options, "Command2", "button_right");
        AddButton(root, options, "Command3", "button_left");

        if (options.Count == 0)
        {
            processing = false;
            yield break;
        }

        ActionRegisterMessage.CreateRegisterMessage(options);
        ActionforceMessage.CreateForceMessage(options);
        previous_options = options;
    }

    private static List<BaseAction> AddButton(Transform root, List<BaseAction> options, string buttonName, string key)
    {
        var buttonObj = root.Find(buttonName)?.gameObject; // Find button object
        if (buttonObj == null || !buttonObj.activeSelf)
            return options;
        
        string costText = root.transform.Find(buttonName + "/TimeIcon/Time/TimeText")?.GetComponent<TextMeshPro>().text;
        string itemText = root.transform.Find(buttonName + "/ItemIcon/Base/ItemText")?.GetComponent<TextMeshPro>().text;
        string text = root.transform.Find(buttonName + "/Text")?.GetComponent<TextMeshPro>().text;

        itemText = itemText == "Item" ? "None" : itemText;
        costText = costText == "00" ? "None" : costText;

        string final_text = $"Action: {text}, Time cost: {costText} seconds, Timie gain: {itemText}, Timie use: None";

        BaseAction newAction = new BaseAction(key, TextCleaner.Clean(final_text));
        options.Add(newAction);
        return options;
    }
    private static List<BaseAction> AddButtonWithTimie(Transform root, List<BaseAction> options, string buttonName, string key, string timiePosition, string timieValue)
    {
        var buttonObj = root.Find(buttonName)?.gameObject; // Find button object
        if (buttonObj == null || !buttonObj.activeSelf)
            return options;
        
        string costText = root.transform.Find(buttonName + "/TimeIcon/Time/TimeText")?.GetComponent<TextMeshPro>().text;
        string itemText = root.transform.Find(buttonName + "/ItemIcon/Base/ItemText")?.GetComponent<TextMeshPro>().text;
        string text = root.transform.Find(buttonName + "/Text")?.GetComponent<TextMeshPro>().text;

        string final_text = $"Action: {text}, Time cost: {costText}, Timie gain: {itemText}, Timie use: {timieValue}";
        string final_key = key + "_" + timiePosition;

        BaseAction newAction = new BaseAction(final_key, TextCleaner.Clean(final_text));
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
