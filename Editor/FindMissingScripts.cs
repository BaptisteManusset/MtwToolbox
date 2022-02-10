using UnityEditor;
using UnityEngine;

namespace ItsBaptiste.Toolbox.Editor {
    public class FindMissingScripts : EditorWindow {
        private static int _goCount, _componentsCount, _missingCount;

        public void OnGUI() {
            if (GUILayout.Button("Find Missing Scripts in selected GameObjects")) FindInSelected();
        }

        [MenuItem("Tools/Find Missing Scripts")]
        public static void ShowWindow() {
            GetWindow(typeof(FindMissingScripts));
        }

        private static void FindInSelected() {
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();


            // GameObject[] go = Selection.gameObjects;
            GameObject[] go = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            _goCount = 0;
            _componentsCount = 0;
            _missingCount = 0;
            foreach (GameObject g in go) FindInGO(g);

            Debug.Log($"Searched {_goCount} GameObjects, {_componentsCount} components, found {_missingCount} missing");
        }

        private static void FindInGO(GameObject g) {
            _goCount++;
            Component[] components = g.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++) {
                _componentsCount++;
                if (components[i] != null) continue;
                
                _missingCount++;
                string s = g.name;
                Transform t = g.transform;
                while (t.parent != null) {
                    s = $"{t.parent.name}/{s}";
                    t = t.parent;
                }

                Debug.Log($"{s} has an empty script attached in position: {i}", g);
            }

            // Now recurse through each child GO (if there are any):
            foreach (Transform childT in g.transform) //Debug.Log("Searching " + childT.name  + " " );
                FindInGO(childT.gameObject);
        }
    }
}