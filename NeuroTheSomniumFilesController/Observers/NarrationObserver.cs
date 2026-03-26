namespace NeuroTheSomniumFiles;

using System;
using UnityEngine;
using TMPro;

public class NarrationObserver : BaseObserver
{

    private TextMeshProUGUI message;
    private string lastLine;

    private string canvas;
    private string location;

    public event Action<string> OnNarration;

    public NarrationObserver(string canvas, string location)
    {
        this.canvas = canvas;
        this.location = location;
    }

    public override void Collect(bool allowSearch, bool loaded) 
    {
        if (allowSearch && message == null)  message = GameObject.Find($"$Root/{canvas}/ScreenScaler/UIOff1/PanelNode/{location}/GameObject/Background/Text")?.GetComponent<TextMeshProUGUI>();
        if (message == null) return;

        string narrationText = message.text;
        if (string.IsNullOrEmpty(narrationText) || narrationText == lastLine) return;

        lastLine = narrationText;

        if (loaded == false) return;
        
        OnNarration?.Invoke($"Narration text: {narrationText}");

    }
}