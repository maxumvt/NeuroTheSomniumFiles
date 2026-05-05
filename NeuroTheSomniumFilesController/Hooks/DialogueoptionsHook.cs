namespace NeuroTheSomniumFiles;

using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[HarmonyPatch(typeof(GameObject), "SetActive")]
public static class Dialogue_SetActive_Patch
{
    private static readonly HashSet<string> moveable = new()
    {
        "zoom_button", "xray_button", "night_vision_button",
        "zoom_night_vision_button", "zoom_xray_button"
    };

    private static List<BaseAction> previous_options = new();

    static void Postfix(GameObject __instance, bool __0)
    {
        try {
            if (!(__instance.name=="Look"))
                return;
            
            if (!__0 && previous_options.Count != 0)
            {
                ResetOptions();
                return;
            }

            CoroutineRunner.Run(CollectNextFrame(__instance));

        }
        catch (Exception ex) {
            Debug.LogError("[DialogueHook] Exception: " + ex);
        }
    }

    private static IEnumerator CollectNextFrame(GameObject instance)
    {
        yield return null; // wait 1 frame

        List<BaseAction> options = new();

        var root = instance.transform.parent;
        string focusTerm = root.Find("Term/Background/Text")?.GetComponent<TextMeshProUGUI>()?.text ?? "";
        
        options.Add(new BaseAction("look_at_term", $"Look at {focusTerm}"));
        
        AddButton(root, options, "SelectU", "button_up");
        AddButton(root, options, "SelectD", "button_down");
        AddButton(root, options, "SelectL", "button_left");
        AddButton(root, options, "SelectR", "button_right");
        AddButton(root, options, "Zoom", "zoom_button", $"Zoom into {focusTerm}");
        AddButton(root, options, "Thermo", "thermo_button", $"Thermo vision on {focusTerm}");
        AddButton(root, options, "XRay", "xray_button", $"XRay vision on {focusTerm}");
        AddButton(root, options, "NV", "night_vision_button", $"Night vision on {focusTerm}");
        AddButton(root, options, "ZoomThermo", "zoom_thermo_button");
        AddButton(root, options, "ZoomXRay", "zoom_xray_button");
        AddButton(root, options, "ZoomNV", "zoom_night_vision_button");

        if (options.Count > 5)
            yield break;

        string formatted = $"Looking at {focusTerm}";
        ContextMessage cMSG = new ContextMessage(formatted, false);
        NetworkClient.SendString(JSON.ToJson(cMSG.message));

        ActionRegisterMessage armMSG = new ActionRegisterMessage(options);
        NetworkClient.SendString(JSON.ToJson(armMSG.message));
        ActionforceMessage afmMSG = new ActionforceMessage(options);
        NetworkClient.SendString(JSON.ToJson(afmMSG.message));
        previous_options = options;
        
    }

    private static List<BaseAction> AddButton(Transform root, List<BaseAction> options, string buttonName, string key, string customText="")
    {
        var buttonRoot = root.Find(buttonName);
        if (buttonRoot == null || !buttonRoot.gameObject.activeSelf)
            return options;

        var buttonObj = root.Find(buttonName)?.gameObject; // Find button object
        if (buttonObj == null || !buttonObj.activeSelf)
            return options;

        string finalKey = key;
        if (moveable.Contains(key))
        {
            var buttonChild = root.Find(buttonName + "/Button");
            if ( buttonChild == null )
                return options;
            bool leftActive  = buttonChild.gameObject.activeSelf;
            finalKey += leftActive ? "_l" : "_r";
        }

        string textComp = root.Find(buttonName + "/Background/Text")?.GetComponent<TextMeshProUGUI>().text; // Store the button GameObject
        BaseAction newAction = new BaseAction(finalKey, textComp != null ? TextCleaner.Clean(textComp) : customText);
        options.Add(newAction);
        return options;
    }

    public static List<BaseAction> GetPreviousActions() { return previous_options; }
    public static void ResetOptions()
    {
        // Send unregister signal
        ActionUnregisterMessage aumMSG = new ActionUnregisterMessage(previous_options);
        NetworkClient.SendString(JSON.ToJson(aumMSG.message));
        
        previous_options.Clear();
    }
}
