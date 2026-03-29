namespace NeuroTheSomniumFiles;

using System;
using UnityEngine;
using TMPro;

public class NarrationObserver : BaseObserver
{
    private GameObject root;

    private TextMeshProUGUI message;
    private string lastLine;

    private string rootPath;
    private string textPath;

    public event Action<string> OnNarration;

    public NarrationObserver(string canvas, string location)
    {
        rootPath = $"$Root/{canvas}/ScreenScaler/UIOff1/PanelNode/{location}/GameObject";
        textPath = "Background/Text";
    }

    public override void Collect(bool allowSearch)
    {
        FindRoot(allowSearch, rootPath, out root);
        if (!root)
            return;

        if (message == null)
        {
            message = FindUIElement<TextMeshProUGUI>(root, textPath);
            if (message != null) ResetUI();
            else return;
        }

        string narrationText = message.text;

        if (narrationText == lastLine || narrationText == placeholder)
            return;
        else { lastLine = narrationText; }

        OnNarration?.Invoke($"Narration text: {narrationText}");
    }

    public override void ResetUI()
    {
        if (message == null)
            return;

        var text_field = message.GetType().GetField("text", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);

        text_field.SetValue(message, placeholder);
    }
}