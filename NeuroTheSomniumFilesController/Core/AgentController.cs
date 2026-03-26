using NeuroTheSomniumFiles;

using UnityEngine;

public class AgentController
{
    private NetworkClient network;
    private GameObservers observations;
    private ActionRegistry actions;
    public AgentController(NetworkClient net, GameObservers obs, ActionRegistry act)
    {
        network = net;
        observations = obs;
        actions = act;
    }

    private float searchTimer = 0f;
    private bool searchAllowed = true;

    private float sceneLoadTimer = 0f;
    private bool sceneLoading = false;
    private bool sceneLoaded = false;

    public void Initialize()
    {
        network.OnMessageReceived += actions.Validate;

        observations.OnBannerText += network.SendString;
        observations.OnLookChoicesUpdated += actions.Register;
        observations.OnTermChange += network.SendString;
        observations.OnLookDisable += actions.Unregister;
        
        observations.sceneObs.OnContext += SceneLoading;

        actions.OnUpdateActionList += network.SendString;
        actions.OnResultMessageCreated += network.SendString;
        
        network.Connect();
        NeuroMessage startUpMsg = new NeuroMessage();
        network.SendString(JSON.ToJson(startUpMsg.message));

    }

    public void Tick()
    {
        searchTimer += Time.deltaTime;
        if ( searchTimer > 1f) { searchAllowed = true; searchTimer = 0f; }
        else searchAllowed = false;

        if (sceneLoading)
        {
            sceneLoadTimer += Time.deltaTime;
            if (sceneLoadTimer > 10f) { sceneLoaded = true; sceneLoading = false; sceneLoadTimer = 0f;}
        }
        
        network.Tick();
        observations.Collect(searchAllowed, sceneLoaded);
    }

    public void SceneLoading(string empty)
    {
        sceneLoading = true;
        sceneLoaded = false;
    }
}