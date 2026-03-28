using NeuroTheSomniumFiles;

using UnityEngine;

public class AgentController
{
    private readonly NetworkClient network;
    private readonly GameObservers observations;
    private readonly ActionRegistry actions;

    private float searchTimer = 0f;
    private bool searchAllowed = true;

    private float sceneLoadTimer = 0f;
    private bool sceneLoading = false;
    private bool sceneLoaded = false;

    private const float SearchInterval = 1f;
    private const float SceneLoadDelay = 10f;

    public AgentController(NetworkClient net, GameObservers obs, ActionRegistry act)
    {
        network = net;
        observations = obs;
        actions = act;
    }


    public void Initialize()
    {
        // Network -> Actions
        network.OnMessageReceived += actions.Validate;

        // Observations -> Network
        observations.OnBannerText += network.SendString;
        observations.OnTermChange += network.SendString;

        // Observations -> Actions
        observations.OnLookChoicesUpdated += actions.Register;
        observations.OnLookDisable += actions.Unregister;
        
        // Scene loading cycle
        observations.sceneObs.OnContext += SceneLoading;

        // Actions -> Network
        actions.OnUpdateActionList += network.SendString;
        actions.OnResultMessageCreated += network.SendString;
        
        network.Connect();

        NeuroMessage startUpMsg = new NeuroMessage();
        network.SendString(JSON.ToJson(startUpMsg.message));

    }

    public void Tick()
    {
        // Search timing
        searchTimer += Time.deltaTime;
        searchAllowed = searchTimer >= SearchInterval;

        if ( searchAllowed )
        {
            searchTimer = 0f;
        }

        // Scene 
        if ( sceneLoading )
        {
            sceneLoadTimer += Time.deltaTime;
            
            if (sceneLoadTimer > SceneLoadDelay)
            {
                sceneLoaded = true;
                sceneLoading = false;
                sceneLoadTimer = 0f;
            }
        }
        
        network.Tick();
        observations.Collect(searchAllowed, sceneLoaded);
    }

    public void SceneLoading(string _)
    {
        sceneLoading = true;
        sceneLoaded = false;
    }
}