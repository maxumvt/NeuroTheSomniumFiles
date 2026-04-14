namespace NeuroTheSomniumFiles;

using UnityEngine;

public class BaseObserver
{
    public virtual void Collect(bool allowSearch, bool loaded) { }
    
     public void FindRoot(bool allowSearch, string path, out GameObject root)
    {
        root = null;
        if (! allowSearch)
            return;

        root = GameObject.Find(path);
        if (! root)
            return;
    }

    public T FindUIElement<T>(GameObject root, string path) where T : Component
    {
        return root.transform.Find(path)?.GetComponent<T>();
    }
}