namespace NeuroTheSomniumFiles;

using System;
using System.Collections.Generic;

public class GameObserver
{
    private List<BaseObserver> observers = new List<BaseObserver>();

    public event Action<string> OnTermChange;
    public event Action<string> OnBannerText;
    public event Action OnLookDisable;
    public event Action<Dictionary<string, string>> OnLookChoicesUpdated;

    public GameObserver() //Instantiate the observers
    {
        // Investigation
        AddDialogue("UICanvas", "MessageWindow");
        AddNarration("UICanvas", "NarrationWindow");
        AddDialogue("UICanvas", "EventMessageWindow");
        AddNarration("UICanvas", "EventNarrationWindow");
        AddDialogue("UICanvas", "FlashBackWindow", "said");
        AddDialogue("UICanvas", "SubtitleWindow");
        AddNarration("UICanvas", "Lyrics");
        var inv = new InvestigationOptionsObserver();
        inv.OnTermChange += (msg) => OnTermChange?.Invoke(msg);
        inv.OnDisable += () => OnLookDisable?.Invoke();
        inv.OnOptionsUpdated += (opts) => OnLookChoicesUpdated?.Invoke(opts);
        observers.Add(inv);

        // Somnium
        AddDialogue("Canvas (1)", "MessageWindow");
        AddNarration("Canvas (1)", "NarrationWindow");
        AddDialogue("Canvas (1)", "EventMessageWindow");
        AddNarration("Canvas (1)", "EventNarrationWindow");
        AddDialogue("Canvas (1)", "FlashBackWindow", "said");
        AddDialogue("Canvas (1)", "SubtitleWindow");
        AddNarration("Canvas (1)", "Lyrics");
        var som = new SomniumOptionsObserver();
        som.OnOptionsUpdated += (opts) => OnLookChoicesUpdated?.Invoke(opts);
        som.OnDisable += () => OnLookDisable?.Invoke();
        observers.Add(som);
    }

    public void Collect(bool searchAllowed)
    {
        foreach (var obs in observers)
        {
            obs.Collect(searchAllowed);
        }
    }

    private void AddDialogue(string canvas, string location, string verb = "says")
    {
        var obs = new DialogueObserver(canvas, location, verb);
        obs.OnDialogue += SendBannerText;
        observers.Add(obs);
    }
    private void AddNarration(string canvas, string location)
    {
        var obs = new NarrationObserver(canvas, location);
        obs.OnNarration += SendBannerText;
        observers.Add(obs);
    }

    private void SendBannerText(string message)
    {
        message = TextCleaner.Clean(message);
        ContextMessage cMsg = new ContextMessage(message, false);
        OnBannerText?.Invoke(cMsg.ToJson());
    }
}
