namespace NeuroTheSomniumFiles;

using BepInEx;
using HarmonyLib;

[BepInPlugin("com.maxum.dialoglogger", "NeuroTheSomniumFiles", "1.0.0")]
public class MyPlugin : BaseUnityPlugin
{
    private AgentController agent;

    void Awake()
    {
        // Core
        var network = new NetworkClient();
        var observations = new GameObservers();

        // Controller
        var actionExecutor = new ActionExecutor();
        var actions = new ActionRegistry(actionExecutor);

        // Apply Harmony patches
        var harmony = new Harmony("neuro.somnium.dialoguehook");
        harmony.PatchAll();

        agent = new AgentController(network, observations, actions);
        agent.Initialize();
    }

    void Update()
    {
        agent?.Tick();
    }
}
