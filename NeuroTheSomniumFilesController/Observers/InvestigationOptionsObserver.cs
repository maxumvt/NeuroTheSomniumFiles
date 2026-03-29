namespace NeuroTheSomniumFiles;

using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InvestigationOptionsObserver : BaseObserver
{
    private GameObject root;
    private TextMeshProUGUI term;
    private bool interactionLastState;

    private List<BaseAction> listedOptions = new();
    private string focusTerm;

    public event Action<string> OnTermChange;
    public event Action OnDisable;
    public event Action<List<BaseAction>> OnOptionsUpdated;
    
    private string rootPath = "$Root/CommandCanvas/ScreenScaler/Command/Scale";
    private string termPath = "Term/Background/Text";

    private readonly HashSet<string> moveable = new()
    {
        "zoom_button", "xray_button", "night_vision_button",
        "zoom_night_vision_button", "zoom_xray_button"
    };


    public override void Collect(bool allowSearch)
    {
        FindRoot(allowSearch, rootPath, out root);
        if ( !root )
            return;

        if ( term == null)
        {
            term = FindUIElement<TextMeshProUGUI>( root, termPath );
            if ( term != null )
                ResetUI();
        }

        bool lookActive = root.transform.Find("Look").gameObject.activeSelf;

        if (interactionLastState == lookActive)
            return;
        if (lookActive == false) 
        {
            ResetUI();
            interactionLastState = false;
            OnDisable?.Invoke();
            return;
        }

        listedOptions.Clear();

        string termText = term?.text;
        focusTerm = termText ?? placeholder;

        if (focusTerm == placeholder)
            return;

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

        ContextMessage msg = new ContextMessage($"Looking at {focusTerm} from investigation", false);
        OnTermChange?.Invoke(JSON.ToJson(msg.message));

        OnOptionsUpdated?.Invoke(listedOptions);
    }


    private void AddButton(string buttonName, string key, string customText="")
    {
        var buttonObj = root.transform.Find(buttonName)?.gameObject; // Find button object
        if (buttonObj == null || !buttonObj.activeSelf)
            return;

        string finalKey = key;
        if (moveable.Contains(key))
        {
            var buttonChild = root.transform.Find(buttonName + "/Button");
            if ( buttonChild == null )
                return;
            bool leftActive  = buttonChild.gameObject.activeSelf;
            finalKey += leftActive ? "_l" : "_r";
        }

        string textComp = root.transform.Find(buttonName + "/Background/Text")?.GetComponent<TextMeshProUGUI>().text; // Store the button GameObject

        if (textComp == placeholder) return;

        BaseAction newAction = new BaseAction(finalKey, textComp != null ? TextCleaner.Clean(textComp) : customText);
        listedOptions.Add(newAction);

    }

    public override void ResetUI()
    {
        if ( term == null )
            return;
        
        interactionLastState = false;
        focusTerm = placeholder;
        
        listedOptions.Clear();

        term.text = placeholder;
    }


}