namespace NeuroTheSomniumFiles;

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueObserver : BaseObserver
{
    private RawImage namePlate;
    private TextMeshProUGUI dialogue;
    private string lastLine;

    private string canvas;
    private string location;
    private string verb;

    public event Action<string> OnDialogue;

    public DialogueObserver(string canvas, string location, string verb = "says")
    {
        this.canvas = canvas;
        this.location = location;
        this.verb = verb;
    }

    public override void Collect(bool allowSearch)
    {
        if (allowSearch && namePlate == null) {namePlate = GameObject.Find($"$Root/{canvas}/ScreenScaler/UIOff1/PanelNode/{location}/Rig/Name/Text")?.GetComponent<RawImage>();} // Necessary for finding the object
        if (allowSearch && dialogue == null) {dialogue = GameObject.Find($"$Root/{canvas}/ScreenScaler/UIOff1/PanelNode/{location}/Rig/Background/Text")?.GetComponent<TextMeshProUGUI>();} // Necessary for finding the object

        if (namePlate == null || dialogue == null) return;

        string dialogueText = dialogue.text;
        string nameText = namePlate.mainTexture.name;
        if (string.IsNullOrEmpty(dialogueText) || dialogueText == lastLine) return;

        lastLine = dialogueText;

        OnDialogue?.Invoke($"{nameText} {verb}: {dialogueText} from: {location}");
        Debug.Log(lastLine);
    }
}
