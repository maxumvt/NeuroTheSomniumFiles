namespace NeuroTheSomniumFiles;

using UnityEngine;
using TMPro;
using System;

class PurposeObserver : BaseObserver
{
    GameObject purposeRoot;
    string lastPurpose;
    string lastTitle;
    string lastBriefing;

    public event Action<string> OnMissionPurpose;

    public PurposeObserver()
    {
        
    }

    public override void Collect(bool allowSearch)
    {
        if (allowSearch && purposeRoot == null) purposeRoot = GameObject.Find($"$Root/Canvas (1)/ScreenScaler/Purpose");

        if (purposeRoot == null) return;

        string purpose = purposeRoot.transform.Find("Text")?.GetComponent<TextMeshProUGUI>().text;
        string title = purposeRoot.transform.Find("ChapterTitle/Text_title")?.GetComponent<TextMeshProUGUI>().text;
        string briefing = purposeRoot.transform.Find("Briefing/BriefingText")?.GetComponent<TextMeshProUGUI>().text;
        if (string.IsNullOrEmpty(purpose) || string.IsNullOrEmpty(title) || string.IsNullOrEmpty(briefing)) return;

        lastPurpose = purpose;
        lastTitle = title;
        lastBriefing = briefing;

        OnMissionPurpose?.Invoke(""); // Fill in with title, purpose, briefing

    }
}