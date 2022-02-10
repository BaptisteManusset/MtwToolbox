using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ItsBaptiste.Toolbox.Tools {
    public static class SaveProjectWhenUnfocusingUnity {
        private static bool _wasFocused;

        [InitializeOnLoadMethod]
        private static void Init() {
            Debug.Log("SaveProjectWhenUnfocusingUnity Enable");
            EditorApplication.update -= CheckApplicationFocus;
            EditorApplication.update += CheckApplicationFocus;
        }

        private static void CheckApplicationFocus() {
            bool isFocused = InternalEditorUtility.isApplicationActive;

            if (isFocused == false && _wasFocused) {
                AssetDatabase.SaveAssets();
            }
            _wasFocused = isFocused;
        }
    }
}