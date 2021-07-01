using UnityEditor;
using UnityEngine;

namespace Toolbox.Editor {
    public static class SaveProjectWhenUnfocusingUnity {
        static bool wasFocused;

        [InitializeOnLoadMethod]
        static void Init() {
            EditorApplication.update -= CheckApplicationFocus;
            EditorApplication.update += CheckApplicationFocus;
        }

        static void CheckApplicationFocus() {
            bool isFocused = UnityEditorInternal.InternalEditorUtility.isApplicationActive;

            if (isFocused == false && wasFocused) {
                AssetDatabase.SaveAssets();
                Debug.Log("<color=cyan>Project AutoSave</color>");
            }

            wasFocused = isFocused;
        }
    }
}