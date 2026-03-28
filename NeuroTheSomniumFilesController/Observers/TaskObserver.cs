namespace NeuroTheSomniumFiles;

using System;
using UnityEngine;
using TMPro;

public class TaskObserver : BaseObserver
{

    private TextMeshProUGUI task;
    private string lastTask;

    public event Action<string> OnTask;

    public override void Collect(bool allowSearch, bool loaded) 
    {
        if ( allowSearch && task == null )  task = GameObject.Find($"$Root/Canvas (1)/ScreenScaler/PurposeRoot/@PurposeInGame(Clone)/base/Text")?.GetComponent<TextMeshProUGUI>();
        if ( task == null ) return;

        string taskText = task.text;
        if ( string.IsNullOrEmpty(taskText) || taskText == lastTask )
            return;

        lastTask = taskText;

        if ( loaded == false )
            return;
        
        OnTask?.Invoke($"Current task: {taskText}");

    }
}