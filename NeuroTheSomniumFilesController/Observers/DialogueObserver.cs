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
    private string namePath;
    private string dialoguePath;

    public event Action<string> OnDialogue;

    public DialogueObserver(string canvas, string location, string verb = "says")
    {
        this.canvas = canvas;
        this.location = location;
        this.verb = verb;

        namePath = $"$Root/{canvas}/ScreenScaler/UIOff1/PanelNode/{location}/Rig/Name/Text";
        dialoguePath = $"$Root/{canvas}/ScreenScaler/UIOff1/PanelNode/{location}/Rig/Background/Text";
    }

    public override void Collect(bool allowSearch, bool loaded)
    {
        if ( allowSearch && namePlate == null ) { namePlate = FindUIElement<RawImage>( namePath ); } // Necessary for finding the object
        if ( allowSearch && dialogue == null ) { dialogue = FindUIElement<TextMeshProUGUI>( dialoguePath ); } // Necessary for finding the object

        if ( namePlate == null || dialogue == null )
            return;

        string dialogueText = dialogue.text;
        string nameText = namePlate.mainTexture != null
            ? namePlate.mainTexture.name
            : "Error";

        if ( string.IsNullOrEmpty(dialogueText) || dialogueText == lastLine )
            return;

        lastLine = dialogueText;

        if ( !loaded )
            return;

        OnDialogue?.Invoke($"{nameText} {verb}: {dialogueText} ({location})");
        Debug.Log(lastLine);
    }
}
