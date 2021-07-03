using UnityEditor;
using UnityEngine;

namespace Toolbox.Editor {
    public class FindMissingScripts : EditorWindow {
        private static int goCount, componentsCount, missingCount;

        public void OnGUI() {
            if (GUILayout.Button("Find Missing Scripts in selected GameObjects")) FindInSelected();
        }

        [MenuItem("Window/FindMissingScripts")]
        public static void ShowWindow() {
            GetWindow(typeof(FindMissingScripts));
        }

        private static void FindInSelected() {
            GameObject[] go = Selection.gameObjects;
            goCount = 0;
            componentsCount = 0;
            missingCount = 0;
            foreach (GameObject g in go) FindInGO(g);

            Debug.Log($"Searched {goCount} GameObjects, {componentsCount} components, found {missingCount} missing");
        }

        private static void FindInGO(GameObject g) {
            goCount++;
            Component[] components = g.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++) {
                componentsCount++;
                if (components[i] == null) {
                    missingCount++;
                    string s = g.name;
                    Transform t = g.transform;
                    while (t.parent != null) {
                        s = t.parent.name + "/" + s;
                        t = t.parent;
                    }

                    Debug.Log($"{s} has an empty script attached in position: {i}", g);
                }
            }

            // Now recurse through each child GO (if there are any):
            foreach (Transform childT in g.transform) //Debug.Log("Searching " + childT.name  + " " );
                FindInGO(childT.gameObject);
        }
    }
}