namespace NeuroTheSomniumFiles;

using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SomniumOptionsObserver : BaseObserver
{
    private GameObject optionsObject;
    private bool interactionActive;

    private Dictionary<string, string> currentOptions = new Dictionary<string, string>();

    public event Action OnDisable;
    public event Action<Dictionary<string, string>> OnOptionsUpdated;

    public override void Collect(bool allowSearch)
    {
        if (optionsObject == null)
        {
            if (!allowSearch) return;

            optionsObject = GameObject.Find("$Root/3DUI");
            if (optionsObject == null) return;
        }

        bool anyActive = false;
        if (optionsObject.activeSelf)
        {
            for (int i = 0; i < 3; i++)
            {
                if (optionsObject.transform.GetChild(i).gameObject.activeSelf)
                {
                    anyActive = true;
                    break;
                }
            }
        }

        if (!anyActive)
        {
            OnDisable?.Invoke();
            if (!interactionActive) return;
            interactionActive = false;
            return;
        }
        if (interactionActive == anyActive) return;
        
        interactionActive = anyActive;
        currentOptions.Clear();
        AddButton("Command1", "button_up");
        AddButton("Command3", "button_left");
        AddButton("Command2", "button_right");

        OnOptionsUpdated?.Invoke(currentOptions);
    }

    private void AddButton(string name, string key)
    {
        var obj = optionsObject.transform.Find(name)?.gameObject;
        if (obj == null || !obj.activeSelf) return;

        var text = optionsObject.transform.Find(name + "/Text")?.GetComponent<TextMeshPro>();
        if (text != null) currentOptions[key] = text.text;
    }
    
}
