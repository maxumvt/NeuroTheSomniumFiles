namespace NeuroTheSomniumFiles;

using System;
using System.Collections.Generic;
using UnityEngine;

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
        sceneObs = new SceneObserver();
        sceneObs.OnContext += SendBannerText;
        observers.Add(sceneObs);

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

        var purpose = new PurposeObserver();
        purpose.OnMissionPurpose += SendBannerText;
        observers.Add(purpose);
    }

    public void Collect(bool searchAllowed)
    {
        string scene = sceneObs.scene ?? "";
        string sceneLower = scene.ToLower();
        

        foreach (var obs in observers)
        {
            if (obs is InvestigationOptionsObserver)
            {
                if (sceneLower.ToLower().Contains("somnium") == true) continue;
            } 
            if (obs is SomniumOptionsObserver)
            {
                if (sceneLower.ToLower().Contains("somnium") == false) continue;
            } 
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
        OnBannerText?.Invoke(JSON.ToJson(cMsg.message));
    }
}
