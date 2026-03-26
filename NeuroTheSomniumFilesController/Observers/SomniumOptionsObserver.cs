namespace NeuroTheSomniumFiles;

using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SomniumOptionsObserver : BaseObserver
{
    private GameObject optionsObject;
    private bool interactionActive;

    private List<BaseAction> listedOptions = new List<BaseAction>(){};

    public event Action OnDisable;
    public event Action<List<BaseAction>> OnOptionsUpdated;

    public override void Collect(bool allowSearch, bool loaded)
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
        listedOptions.Clear();
        AddButton("Command1", "button_up");
        AddButton("Command3", "button_left");
        AddButton("Command2", "button_right");

        if (loaded == false) return;

        OnOptionsUpdated?.Invoke(listedOptions);
    }

    private void AddButton(string name, string key)
    {
        var obj = optionsObject.transform.Find(name)?.gameObject;
        if (obj == null || !obj.activeSelf) return;

        string text = optionsObject.transform.Find(name + "/Text")?.GetComponent<TextMeshPro>().text;
        BaseAction newAction = new BaseAction(key, text);
        listedOptions.Add(newAction);
    }
    
}
