namespace NeuroTheSomniumFiles;

using System;
using UnityEngine;
using TMPro;

public class MentalLockObserver : BaseObserver
{

    private TextMeshProUGUI mentalLock;
    private string lastLock;

    public event Action<string> OnMentalLock;

    public override void Collect(bool allowSearch, bool loaded) 
    {
        if ( allowSearch && mentalLock == null )  mentalLock = GameObject.Find($"$Root/MiddletCanvas/ScreenScaler/LockClear/Text")?.GetComponent<TextMeshProUGUI>();
        if ( mentalLock == null )
            return;

        string mentalLockText = mentalLock.text;
        if ( string.IsNullOrEmpty(mentalLockText) || mentalLockText == lastLock )
            return;

        lastLock = mentalLockText;

        if ( loaded == false )
            return;
        
        OnMentalLock?.Invoke($"Mental lock cleared: {mentalLockText}");

    }
}