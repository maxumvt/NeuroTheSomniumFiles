namespace NeuroTheSomniumFiles;

using System;
using UnityEngine;
using TMPro;

public class TaskObserver : BaseObserver
{
    private GameObject root;

    private TextMeshProUGUI task;
    private string lastTask;

    private string rootPath;
    private string textPath;

    public event Action<string> OnTask;

    public TaskObserver()
    {
        rootPath = "$Root/Canvas (1)/ScreenScaler/PurposeRoot/@PurposeInGame(Clone)/base";
        textPath = "Text";
    }

    public override void Collect(bool allowSearch)
    {
        FindRoot(allowSearch, rootPath, out root);
        if (!root)
            return;

        if (task == null)
        {
            task = FindUIElement<TextMeshProUGUI>(root, textPath);
            if (task != null) ResetUI();
            else return;
        }

        string taskText = task.text;

        if (taskText == lastTask || taskText == placeholder)
            return;
        else { lastTask = taskText; }

        OnTask?.Invoke($"Current task: {taskText}");
    }

    public override void ResetUI()
    {
        if (task == null)
            return;

        var text_field = task.GetType().GetField("text", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public );
        text_field.SetValue(task, placeholder);
    }
}