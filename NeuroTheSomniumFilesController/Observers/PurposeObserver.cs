namespace NeuroTheSomniumFiles;

using UnityEngine;
using TMPro;
using System;

class PurposeObserver : BaseObserver
{
    GameObject purposeRoot;
    string lastPurpose;
    string lastBriefing;

    public event Action<string> OnMissionPurpose;

    public PurposeObserver()
    {
        if (purposeRoot == null) purposeRoot = GameObject.Find($"$Root/Canvas (1)/ScreenScaler/Purpose");
        if (purposeRoot) purposeRoot.SetActive(false);
    }

    public override void Collect(bool allowSearch)
    {
        if (allowSearch && purposeRoot == null) purposeRoot = GameObject.Find($"$Root/Canvas (1)/ScreenScaler/Purpose");

        if (purposeRoot == null) return;
        if (purposeRoot.gameObject.activeSelf == false) return;

        string purpose = purposeRoot.transform.Find("Text")?.GetComponent<TextMeshProUGUI>().text;
        string title = purposeRoot.transform.Find("ChapterTitle/Text_title")?.GetComponent<TextMeshProUGUI>().text;
        string briefing = purposeRoot.transform.Find("Briefing/BriefingText")?.GetComponent<TextMeshProUGUI>().text;
        if (string.IsNullOrEmpty(purpose) || string.IsNullOrEmpty(title) || string.IsNullOrEmpty(briefing) || purpose == lastPurpose || briefing == lastBriefing) return;

        lastPurpose = purpose;
        lastBriefing = briefing;

        OnMissionPurpose?.Invoke($"Mission title: {title}, Mission purpose: {purpose}, Mission briefing: {briefing}"); // Fill in with title, purpose, briefing

    }
}