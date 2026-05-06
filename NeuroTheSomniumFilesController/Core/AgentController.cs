using NeuroTheSomniumFiles;

using UnityEngine;

public class AgentController
{
    private readonly NetworkClient network;
    private readonly ActionRegistry actions;

    public AgentController(NetworkClient net, ActionRegistry act)
    {
        network = net;
        actions = act;
    }


    public void Initialize()
    {
        // Network -> Actions
        network.OnMessageReceived += actions.Validate;
        network.Connect();

        // Start up message
        NeuroMessage startUpMsg = new NeuroMessage();
        NetworkClient.SendString(JSON.ToJson(startUpMsg.message));

    }

    public void Tick()
    {
        network.Tick();
    }
}