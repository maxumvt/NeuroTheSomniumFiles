namespace NeuroTheSomniumFiles;

using UnityEngine;
using Game;
using System;

public class SceneObserver : BaseObserver
{
    public string scene;
    private string lastScene;

    public event Action<string> OnContext;

    public override void Collect(bool allowSearch)
    {
        if (RootResources.instance == null) return;

        scene = RootResources.instance.startScript;
        if (string.IsNullOrEmpty(scene) || lastScene == scene) return;

        lastScene = scene;

        OnContext?.Invoke($"Scene changed to {scene}");
    }
}