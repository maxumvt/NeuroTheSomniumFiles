namespace NeuroTheSomniumFiles;

using System;
using UnityEngine;
using TMPro;

public class MentalLockObserver : BaseObserver
{
    private GameObject root;

    private TextMeshProUGUI mentalLock;
    private string lastLock;

    private string rootPath;
    private string textPath;

    public event Action<string> OnMentalLock;

    public MentalLockObserver()
    {
        rootPath = "$Root/MiddletCanvas/ScreenScaler/LockClear";
        textPath = "Text";
    }

    public override void Collect(bool allowSearch)
    {
        FindRoot(allowSearch, rootPath, out root);
        if (!root)
            return;

        if (mentalLock == null)
        {
            mentalLock = FindUIElement<TextMeshProUGUI>(root, textPath);
            if (mentalLock != null) ResetUI();
            else return;
        }

        string mentalLockText = mentalLock.text;

        if (mentalLockText == lastLock || mentalLockText == placeholder || mentalLockText == "")
            return;
        else { lastLock = mentalLockText; }

        OnMentalLock?.Invoke($"Mental lock cleared: {mentalLockText}");
    }

    public override void ResetUI()
    {
        if (mentalLock == null)
            return;

        mentalLock.text = placeholder;
    }
}