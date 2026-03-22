namespace NeuroTheSomniumFiles;

using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InvestigationOptionsObserver : BaseObserver
{
    private GameObject optionsObject;
    private bool interactionActive;

    private Dictionary<string, string> currentOptions = new Dictionary<string, string>();
    private string focusTerm;

    public event Action<string> OnTermChange;
    public event Action OnDisable;
    public event Action<Dictionary<string, string>> OnOptionsUpdated;

    private List<string> moveable = new List<string>()
    {
        "zoom_button", "xray_button", "night_vision_button",
        "zoom_night_vision_button", "zoom_xray_button"
    };


    public override void Collect(bool allowSearch)
    {
        if (optionsObject == null)
        {
            if (!allowSearch) return;

            optionsObject = GameObject.Find("$Root/CommandCanvas/ScreenScaler/Command/Scale");
            if (optionsObject == null) return;
        }

        bool lookActive = optionsObject.transform.Find("Look").gameObject.activeSelf;
        if (interactionActive == lookActive) return; // no change

        interactionActive = lookActive;
        currentOptions.Clear();
        if (!lookActive)
        {
            OnDisable?.Invoke();
            focusTerm = "";
            return;
        }

        string termText = optionsObject.transform.Find("Term/Background/Text")?.GetComponent<TextMeshProUGUI>()?.text;
        focusTerm = termText ?? "";

        ContextMessage msg = new ContextMessage($"Looking at {focusTerm}", false);
        OnTermChange?.Invoke(msg.ToJson());

        currentOptions["look_at_term"] = $"Look at {focusTerm}";

        AddButton("SelectU", "button_up");
        AddButton("SelectD", "button_down");
        AddButton("SelectL", "button_left");
        AddButton("SelectR", "button_right");
        AddButton("Zoom", "zoom_button", $"Zoom into {focusTerm}");
        AddButton("Thermo", "thermo_button", $"Thermo vision on {focusTerm}");
        AddButton("XRay", "xray_button", $"XRay vision on {focusTerm}");
        AddButton("NV", "night_vision_button", $"Night vision on {focusTerm}");
        AddButton("ZoomThermo", "zoom_thermo_button");
        AddButton("ZoomXRay", "zoom_xray_button");
        AddButton("ZoomNV", "zoom_night_vision_button");

        if (currentOptions.Count == 0) return;

        OnOptionsUpdated?.Invoke(currentOptions);
    }


    private void AddButton(string buttonName, string key, string customText="")
    {
        var buttonObj = optionsObject.transform.Find(buttonName)?.gameObject; // Find button object
        if (buttonObj == null || !buttonObj.activeSelf) return;

        string finalKey = key;
        if (moveable.Contains(key))
        {
            bool leftActive  = optionsObject.transform.Find(buttonName + "/Button").gameObject.activeSelf;
            finalKey += leftActive ? "_l" : "_r";
        }

        var textComp = optionsObject.transform.Find(buttonName + "/Background/Text")?.GetComponent<TextMeshProUGUI>(); // Store the button GameObject
        currentOptions[finalKey] = textComp != null ? TextCleaner.Clean(textComp.text) : customText;

    }
}
