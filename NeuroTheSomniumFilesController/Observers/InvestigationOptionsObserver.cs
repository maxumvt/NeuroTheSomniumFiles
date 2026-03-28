namespace NeuroTheSomniumFiles;

using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InvestigationOptionsObserver : BaseObserver
{
    private Transform root;
    private bool interactionActive;

    private List<BaseAction> listedOptions = new();
    private string focusTerm;

    public event Action<string> OnTermChange;
    public event Action OnDisable;
    public event Action<List<BaseAction>> OnOptionsUpdated;

    private readonly HashSet<string> moveable = new()
    {
        "zoom_button", "xray_button", "night_vision_button",
        "zoom_night_vision_button", "zoom_xray_button"
    };


    public override void Collect(bool allowSearch, bool loaded)
    {
        if ( root == null )
        {
            if ( !allowSearch )
                return;

            root = GameObject.Find("$Root/CommandCanvas/ScreenScaler/Command/Scale")?.transform;
            if ( root == null )
                return;
        }

        bool lookActive = root.Find("Look").gameObject.activeSelf;
        if (interactionActive == lookActive)
        {
            // OnDisable?.Invoke();
            return;
        }

        interactionActive = lookActive;
        listedOptions.Clear();

        string termText = root.Find("Term/Background/Text")?.GetComponent<TextMeshProUGUI>()?.text;
        focusTerm = termText ?? "";

        if (!lookActive || focusTerm == "???" || focusTerm == "aaa" )
        {
            OnDisable?.Invoke();

            focusTerm = "";
            return;
        }


        BaseAction newAction = new BaseAction("look_at_term", $"Look at {focusTerm}");
        listedOptions.Add(newAction);

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

        if ( listedOptions.Count == 0 )
            return;

        if ( !loaded )
            return;
        
        ContextMessage msg = new ContextMessage($"Looking at {focusTerm} from investigation", false);
        OnTermChange?.Invoke(JSON.ToJson(msg.message));

        OnOptionsUpdated?.Invoke(listedOptions);
    }


    private void AddButton(string buttonName, string key, string customText="")
    {
        var buttonRoot = root.Find(buttonName);
        if (buttonRoot == null || !buttonRoot.gameObject.activeSelf)
            return;

        var buttonObj = root.Find(buttonName)?.gameObject; // Find button object
        if (buttonObj == null || !buttonObj.activeSelf)
            return;

        string finalKey = key;
        if (moveable.Contains(key))
        {
            var buttonChild = root.Find(buttonName + "/Button");
            if ( buttonChild == null )
                return;
            bool leftActive  = buttonChild.gameObject.activeSelf;
            finalKey += leftActive ? "_l" : "_r";
        }

        string textComp = root.Find(buttonName + "/Background/Text")?.GetComponent<TextMeshProUGUI>().text; // Store the button GameObject
        BaseAction newAction = new BaseAction(finalKey, textComp != null ? TextCleaner.Clean(textComp) : customText);
        listedOptions.Add(newAction);

    }
}
