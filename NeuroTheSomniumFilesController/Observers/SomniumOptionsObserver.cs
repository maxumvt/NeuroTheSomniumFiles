namespace NeuroTheSomniumFiles;

using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SomniumOptionsObserver : BaseObserver
{
    private GameObject root;
    private bool interactionLastState;

    private List<BaseAction> listedOptions = new();

    public event Action OnDisable;
    public event Action<List<BaseAction>> OnOptionsUpdated;

    private string rootPath = "$Root/3DUI";

    public override void Collect(bool allowSearch)
    {
        FindRoot(allowSearch, rootPath, out root);
        if (!root)
            return;

        bool anyActive = false;
        if (root.activeSelf)
        {
            for (int i = 0; i < 3; i++)
            {
                if (root.transform.GetChild(i).gameObject.activeSelf)
                {
                    anyActive = true;
                    break;
                }
            }
        }

        if (interactionLastState == anyActive)
            return;

        if (anyActive == false)
        {
            interactionLastState = false;
            listedOptions.Clear();
            OnDisable?.Invoke();
            return;
        }

        interactionLastState = anyActive;

        listedOptions.Clear();

        AddButton("Command1", "button_up");
        AddButton("Command3", "button_left");
        AddButton("Command2", "button_right");

        if (listedOptions.Count == 0)
            return;

        OnOptionsUpdated?.Invoke(listedOptions);
    }

    private void AddButton(string name, string key)
    {
        var obj = root.transform.Find(name)?.gameObject;
        if (obj == null || !obj.activeSelf)
            return;

        string text = root.transform.Find(name + "/Text")?.GetComponent<TextMeshPro>().text;

        if (text == placeholder)
            return;

        BaseAction newAction = new BaseAction(key, text);
        listedOptions.Add(newAction);
    }

    public override void ResetUI()
    {
        interactionLastState = false;
        listedOptions.Clear();
    }
}