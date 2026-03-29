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
    if (string.IsNullOrEmpty(timeText))
        return 0;

    try
    {
        // Remove TMP tags like <scale=...>
        string cleaned = System.Text.RegularExpressions.Regex.Replace(timeText, "<.*?>", "");

        // The string is now like "36000" — first 3 digits = minutes, last 2 = seconds
        if (cleaned.Length < 3) 
            return 0;

        // Split into minutes and seconds
        string minutesStr = cleaned.Substring(0, cleaned.Length - 2);
        string secondsStr = cleaned.Substring(cleaned.Length - 2, 2);

        int minutes = int.Parse(minutesStr);
        int seconds = int.Parse(secondsStr);

        return minutes * 60 + seconds;
    }
    catch
    {
        return 0;
    }
}
}