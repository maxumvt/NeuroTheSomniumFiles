namespace NeuroTheSomniumFiles;

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueObserver : BaseObserver
{
    private GameObject root;

    private RawImage namePlate;
    private TextMeshProUGUI dialogue;
    private string lastLine;

    private string location;
    private string verb;

    private string namePath;
    private string dialoguePath;
    private string rootPath;

    public event Action<string> OnDialogue;

    public DialogueObserver(string canvas, string location, string verb = "says")
    {
        this.location = location;
        this.verb = verb;

        rootPath = $"$Root/{canvas}/ScreenScaler/UIOff1/PanelNode/{location}/Rig";
        namePath = $"Name/Text";
        dialoguePath = $"Background/Text";
    }

    public override void Collect(bool allowSearch)
    {
        FindRoot( allowSearch, rootPath, out root );
        if ( ! root )
            return;

        if ( namePlate == null ) namePlate = FindUIElement<RawImage>( root, namePath );
        if ( dialogue == null )
        {
            dialogue = FindUIElement<TextMeshProUGUI>( root, dialoguePath );
            if ( dialogue != null ) ResetUI();
            else return;
            
        }

        string dialogueText = dialogue.text;
        string nameText = namePlate.mainTexture != null ? namePlate.mainTexture.name : "Error";

        if ( dialogueText == lastLine || dialogueText == placeholder || dialogueText == "" ) return;
        else { lastLine = dialogueText; }

        OnDialogue?.Invoke($"{nameText} {verb}: {dialogueText} ({location})");
        Debug.Log(lastLine);
    }


    public override void ResetUI()
    {
        if ( dialogue == null )
            return;
        
        dialogue.text = placeholder;
    }
    

}