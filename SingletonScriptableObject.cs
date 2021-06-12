using System.Linq;
using UnityEngine;

//https://medium.com/@GilbertoBitt/singleton-scriptableobject-madewithunity-bfe9b8385566
public abstract class SingletonScriptableObject<T> : ScriptableObject where T : ScriptableObject {
    static T _instance = null;
    public static T Instance
    {
        get
        {
            if (!_instance)
                _instance = Resources.FindObjectsOfTypeAll<T>().FirstOrDefault();
            return _instance;
        }
    }
}