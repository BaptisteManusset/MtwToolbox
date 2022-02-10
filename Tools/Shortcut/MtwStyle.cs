using UnityEditor;
using UnityEngine;

namespace ItsBaptiste.Toolbox.Tools.Shortcut {
    public static class MtwStyle {
        public static readonly GUIStyle StyleButton = new GUIStyle(GUI.skin.button) {
            richText = true
        };

        public static readonly GUIStyle StyleFolder = new GUIStyle(GUI.skin.label) {
            richText = true,
            fontSize = 10,
            alignment = TextAnchor.MiddleRight
        };

        public static readonly GUIStyle StyleTxt = new GUIStyle {
            richText = true
        };

        public static bool ButtonIcon(string icon, string tooltip = "", params GUILayoutOption[] options) {
            return GUILayout.Button(
                new GUIContent(EditorGUIUtility.FindTexture(icon), tooltip), options);
        }

        public static bool Label(string path, string name) {
            bool result = false;

            float h = Mathf.Abs((float)(name.GetHashCode() % 10)) / 10;
            GUI.backgroundColor = Color.HSVToRGB(h, 1, 1);
            result = GUILayout.Button(name, EditorStyles.miniButton);
            GUI.backgroundColor = Color.white;

            return result;
        }

        public static void Separator() {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }
    }
}