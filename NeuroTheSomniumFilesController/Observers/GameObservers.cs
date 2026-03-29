namespace NeuroTheSomniumFiles;

using System;
using System.Collections.Generic;

public class GameObservers
{
    // Gamecontroller RootResources startScene and startScript
    private List<BaseObserver> observers = new List<BaseObserver>();

    public event Action<string> OnTermChange;
    public event Action<string> OnBannerText;
    public event Action OnLookDisable;
    public event Action<List<BaseAction>> OnLookChoicesUpdated;

    public SceneObserver sceneObs = new SceneObserver();

    public GameObservers() //Instantiate the observers
    {
        // Game states
        sceneObs.OnContext += SendBannerText;
        sceneObs.OnContext += UIReset;
        observers.Add(sceneObs);

        // Investigation
        AddStandardUISet("UICanvas");

        var inv = new InvestigationOptionsObserver();
        inv.OnTermChange += msg => OnTermChange?.Invoke(msg);
        inv.OnDisable += () => OnLookDisable?.Invoke();
        inv.OnOptionsUpdated += opts => OnLookChoicesUpdated?.Invoke(opts);
        observers.Add(inv);

        // Somnium
        AddStandardUISet("Canvas (1)");

        var taskObs = new TaskObserver();
        taskObs.OnTask += SendBannerText;
        observers.Add(taskObs);

        var mentalObs = new MentalLockObserver();
        mentalObs.OnMentalLock += SendBannerText;
        observers.Add(mentalObs);

        var som = new SomniumOptionsObserver();
        som.OnOptionsUpdated += opts => OnLookChoicesUpdated?.Invoke(opts);
        som.OnDisable += () => OnLookDisable?.Invoke();
        observers.Add(som);

        var purpose = new PurposeObserver();
        purpose.OnMissionPurpose += SendBannerText;
        observers.Add(purpose);
    }

    private void AddStandardUISet(string canvas)
    {
        AddDialogue(canvas, "MessageWindow");
        AddNarration(canvas, "NarrationWindow");
        AddDialogue(canvas, "EventMessageWindow");
        AddNarration(canvas, "EventNarrationWindow");
        AddDialogue(canvas, "FlashBackWindow", "said");
        AddDialogue(canvas, "SubtitleWindow");
        AddNarration(canvas, "Lyrics");
    }

    public void Collect(bool searchAllowed, bool loaded)
    {
        string sceneLower = sceneObs.scene?.ToLowerInvariant() ?? "";
        bool isSomnium = sceneLower.Contains("somnium");

        foreach (var obs in observers)
        {
            bool skip = ( obs is InvestigationOptionsObserver && isSomnium ) || ( obs is SomniumOptionsObserver && !isSomnium );

            if ( skip )
                continue;

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
        string cleaned = TextCleaner.Clean(message);
        ContextMessage cMsg = new ContextMessage(cleaned, false);
        OnBannerText?.Invoke(JSON.ToJson(cMsg.message));
    }

    private void UIReset(string _)
    {
        foreach (var obs in observers)
        {
            obs.ResetUI();
        }
    }
}