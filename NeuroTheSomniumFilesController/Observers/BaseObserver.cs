namespace NeuroTheSomniumFiles;

using UnityEngine;

public class BaseObserver
{
    public virtual void Collect(bool allowSearch, bool loaded) { }
    
    public T FindUIElement<T>(string path) where T : Component
    {
        return GameObject.Find(path)?.GetComponent<T>();
    }
}