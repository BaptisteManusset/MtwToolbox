using UnityEditor;
using UnityEngine;

//https://medium.com/@GilbertoBitt/singleton-scriptableobject-madewithunity-bfe9b8385566
namespace Toolbox {
    public abstract class SingletonScriptableObject<T> : ScriptableObject where T : ScriptableObject {
        private static T s_Instance;

        public static T Instance {
            get {
                if (s_Instance) return s_Instance;

                T[] objs = null;

#if UNITY_EDITOR
                // If we're running the game in the editor, the "Preloaded Assets" array will be ignored.
                // So get all the assets of type T using AssetDatabase.
                string[] objsGuid = AssetDatabase.FindAssets("t:" + typeof(T).Name);
                int count = objsGuid.Length;
                objs = new T[count];
                for (int i = 0; i < count; i++)
                    objs[i] = AssetDatabase.LoadAssetAtPath<T>(
                        AssetDatabase.GUIDToAssetPath(objsGuid[i]));
#else
                // Get all asset of type T from Resources or loaded assets.
                objs = Resources.FindObjectsOfTypeAll<T>();
#endif

                // If no asset of type T was found...
                if (objs.Length == 0)
                    Debug.LogError(
                        $"No asset of type \"{typeof(T).Name}\" has been found in loaded resources. Please create a new one and add it to the \"Preloaded Assets\" array in Edit > Project Settings > Player > Other Settings.");

                // If more than one asset of type T was found...
                else if (objs.Length > 1)
                    Debug.LogError(
                        $"There's more than one asset of type \"{typeof(T).Name}\" loaded in this project. We expect it to have a Singleton behaviour. Please remove other assets of that type from this project.");

                s_Instance = objs.Length > 0 ? objs[0] : null;

                return s_Instance;
            }
        }
    }
}