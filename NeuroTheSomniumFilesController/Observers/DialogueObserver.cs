namespace NeuroTheSomniumFiles;

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WebSocketSharp;

public class DialogueObserver : BaseObserver
{
    private GameObject root;

    private RawImage namePlate;
    private TextMeshProUGUI dialogue;
    private string lastLine;
    private string lastName;

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

    public override void Collect(bool allowSearch, bool loaded)
    {
        if ( ! root )
        {
            FindRoot( allowSearch, rootPath, out root );
            if ( ! root)
                return;
        }

        if ( namePlate == null ) namePlate = FindUIElement<RawImage>( root, namePath );
        if ( dialogue == null )
        {
            dialogue = FindUIElement<TextMeshProUGUI>( root, dialoguePath );
            
            if ( dialogue != null ) { Debug.Log($"Dialogue ref: {dialogue?.GetInstanceID()}"); }
            else return;
        }

        string dialogueText = dialogue.text;
        // var dialogueTextField = dialogue.GetType().GetField("m_text", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
        // string dialogueText = (string)dialogueTextField.GetValue(dialogue);
        string nameText = namePlate.mainTexture != null ? namePlate.mainTexture.name : "Error";

        if ( dialogueText == "" )
            return;

        bool isNewText = dialogueText != lastLine;
        bool isNewName = nameText != lastName;

        lastLine = dialogueText;
        lastName = nameText;

        if ( !isNewName && !isNewText )
            return;


        OnDialogue?.Invoke($"{nameText} {verb}: {dialogueText} ({location})");
        Debug.Log(lastLine);
    }
}
