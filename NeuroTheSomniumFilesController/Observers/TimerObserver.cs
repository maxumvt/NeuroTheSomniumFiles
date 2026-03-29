namespace NeuroTheSomniumFiles;

using System;
using UnityEngine;
using TMPro;


public class TimerObserver : BaseObserver
{

    private TextMeshProUGUI timerTime;

    private int currentTime = 300;
    private int lastSendTime = 0;
    private const int timeDifference = 5;

    public event Action<string> OnTimerSignicantDifference;

    public override void Collect(bool allowSearch) 
    {
        if ( allowSearch && timerTime == null ) timerTime = GameObject.Find($"$Root/MiddletCanvas/ScreenScaler/Timer/Background//Text")?.GetComponent<TextMeshProUGUI>();
        if ( timerTime == null )
            return;

        string timerTimeText = timerTime.text;
        if ( string.IsNullOrEmpty(timerTimeText) )
            return;

        currentTime = CleanTimerText(timerTimeText);

        if (lastSendTime - currentTime >= timeDifference)
        {
            OnTimerSignicantDifference?.Invoke($"Current time left: {currentTime}");
            lastSendTime = currentTime;
        }
    }

    private int CleanTimerText(string timeText)
    {
        int time = 1;
        // regex only the numbers and then add them together in a time way

        return time;
    }
}