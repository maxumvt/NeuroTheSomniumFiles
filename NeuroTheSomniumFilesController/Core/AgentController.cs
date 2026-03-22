using NeuroTheSomniumFiles;

using UnityEngine;

public class AgentController
{
    private NetworkClient network;
    private GameObserver observations;
    private ActionRegistry actions;
    public AgentController(NetworkClient net, GameObserver obs, ActionRegistry act)
    {
        network = net;
        observations = obs;
        actions = act;
    }

    private float searchTimer = 0;
    private bool searchAllowed = true;

    public void Initialize()
    {
        network.OnMessageReceived += actions.Validate;
        observations.OnBannerText += network.SendString;
        observations.OnLookChoicesUpdated += actions.Register;
        observations.OnTermChange += network.SendString;
        actions.OnUpdateActionList += network.SendString;
        actions.OnResultMessageCreated += network.SendString;
        observations.OnLookDisable += actions.Unregister;
        
        network.Connect();
        NeuroMessage startUpMsg = new NeuroMessage();
        network.SendString(startUpMsg.StartupToJson());

    }

    public void Tick()
    {
        searchTimer += Time.deltaTime;
        if ( searchTimer > 1f) { searchAllowed = true; searchTimer = 0f; }
        else searchAllowed = false;
        
        network.Tick();
        observations.Collect(searchAllowed);
    }
}