using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Toolbox.Editor {
    public static class SaveProjectWhenUnfocusingUnity {
        private static bool wasFocused;

        [InitializeOnLoadMethod]
        private static void Init() {
            EditorApplication.update -= CheckApplicationFocus;
            EditorApplication.update += CheckApplicationFocus;
        }

        private static void CheckApplicationFocus() {
            bool isFocused = InternalEditorUtility.isApplicationActive;

            if (isFocused == false && wasFocused) {
                AssetDatabase.SaveAssets();
                Debug.Log("<color=cyan>Project AutoSave</color>");
            }

            wasFocused = isFocused;
        }
    }
}