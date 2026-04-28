namespace NeuroTheSomniumFiles;

using UnityEngine;
using System.Collections;

public class CoroutineRunner : MonoBehaviour
{
    private static CoroutineRunner instance;

    public static void Run(IEnumerator routine)
    {
        if (instance == null)
        {
            var go = new GameObject("CoroutineRunner");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<CoroutineRunner>();
        }

        instance.StartCoroutine(routine);
    }
}