namespace NeuroTheSomniumFiles;

using UnityEngine;
using TMPro;
using System;

class PurposeObserver : BaseObserver
{
    private GameObject root;

    private TextMeshProUGUI purposeText;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI briefingText;

    private string lastPurpose;
    private string lastBriefing;

    private string rootPath;
    private string purposePath;
    private string titlePath;
    private string briefingPath;

    public event Action<string> OnMissionPurpose;

    public PurposeObserver()
    {
        rootPath = "$Root/Canvas (1)/ScreenScaler/Purpose";
        purposePath = "Text";
        titlePath = "ChapterTitle/Text_title";
        briefingPath = "Briefing/BriefingText";
    }

    public override void Collect(bool allowSearch)
    {
        FindRoot(allowSearch, rootPath, out root);
        if (!root || !root.gameObject.activeSelf)
            return;

        if (purposeText == null)
        {
            purposeText = FindUIElement<TextMeshProUGUI>(root, purposePath);
            if (purposeText != null) ResetUI();
            else return;
        }

        if (titleText == null) titleText = FindUIElement<TextMeshProUGUI>(root, titlePath);
        if (briefingText == null) briefingText = FindUIElement<TextMeshProUGUI>(root, briefingPath);

        if (titleText == null || briefingText == null)
            return;

        string purpose = purposeText.text;
        string title = titleText.text;
        string briefing = briefingText.text;

        if (purpose == lastPurpose || briefing == lastBriefing || purpose == placeholder)
            return;
        
        lastPurpose = purpose;
        lastBriefing = briefing;

        OnMissionPurpose?.Invoke($"Mission title: {title}, Mission purpose: {purpose}, Mission briefing: {briefing}");
    }

    public override void ResetUI()
    {
        if (purposeText == null || titleText == null || briefingText == null )
            return;

        titleText.text = placeholder;
        purposeText.text = placeholder;
        briefingText.text = placeholder;
        // var text_field = purposeText.GetType().GetField("text", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public );
        // text_field.SetValue(purposeText, placeholder);
    }
}