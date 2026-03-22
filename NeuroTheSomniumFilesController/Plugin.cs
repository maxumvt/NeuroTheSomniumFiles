namespace NeuroTheSomniumFiles;

using BepInEx;

[BepInPlugin("com.maxum.dialoglogger", "NeuroTheSomniumFiles", "1.0.0")]
public class MyPlugin : BaseUnityPlugin
{
    private AgentController agent;

    void Awake()
    {
        var network = new NetworkClient();
        var actionExecutor = new ActionExecutor();
        var actions = new ActionRegistry(actionExecutor);
        var observations = new GameObserver();

        agent = new AgentController(network, observations, actions);
        agent.Initialize();
    }

    void Update()
    {
        agent.Tick();
    }
}
