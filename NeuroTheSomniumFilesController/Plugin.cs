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

        // Controller
        var actions = new ActionRegistry();

        // Apply Harmony patches
        var harmony = new Harmony("neuro.somnium.dialoguehook");
        harmony.PatchAll();

        agent = new AgentController(network, actions);
        agent.Initialize();
    }

    void Update()
    {
        agent?.Tick();
    }
}
