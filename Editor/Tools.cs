using AssetUsageDetectorNamespace;
using ItsBaptiste.Scriptabbles.RequiredPrefab;
using ItsBaptiste.Scriptabbles.SceneLink;
using Toolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Editor {
    public class Tools : EditorWindow {
        private static int page;

        private void OnGUI() {
            switch (page) {
                default:
                    ViewMain();
                    break;
                case 1:
                    ViewScene();
                    break;
                case 2:
                    ViewToolsMenu();
                    break;
                case 3:
                    ViewSceneTools();
                    break;
            }
        }


        [MenuItem("⚙️ 𝗧𝗢𝗢𝗟𝗦/Tools",priority = -1000)]
        private static void ShowWindow() {
            Tools window = GetWindow<Tools>();
            window.titleContent = new GUIContent("Tools");
            window.Show();
        }

        [MenuItem("🌐 𝗘𝗫𝗧𝗘𝗥𝗡𝗔𝗟/Trello")] private static void OpenTrello() => Application.OpenURL("https://trello.com/b/X2LKhO8w/unity");
        [MenuItem("🌐 𝗘𝗫𝗧𝗘𝗥𝗡𝗔𝗟/Inspiration")] private static void OpenInspi() => Application.OpenURL("https://drive.google.com/drive/u/0/folders/1hBiQjx4Kr3m9LdXd6tDmEmHDpkZWmLQm");
        [MenuItem("🌐 𝗘𝗫𝗧𝗘𝗥𝗡𝗔𝗟/Git")] private static void OpenGit() => Application.OpenURL("https://github.com/BaptisteManusset/ffffffps");


        #region Editor

        private void TopPage(string pageName = "") {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Retour", GUILayout.Height(50), GUILayout.Width(50))) page = 0;
            GUILayout.FlexibleSpace();
            GUILayout.Label(pageName, GUILayout.Height(50));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        #endregion

        #region Views

        private void ViewScene() {
            TopPage("Scene");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Load Menu", GUILayout.Height(50))) {
                SceneLink.LoadMenu();
            }

            if (GUILayout.Button("Load Game", GUILayout.Height(50))) {
                SceneLink.LoadGame();
            }

            GUILayout.EndHorizontal();

            // if (GUILayout.Button("Unload Scenes", GUILayout.Height(50))) SceneLink.UnloadScenes();
            if (GUILayout.Button("Display Asset", GUILayout.Height(50))) SceneLink.Instance.SelectInEditor();
        }

        private void ViewMain() {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Scenes", GUILayout.Height(50))) page = 1;
            if (GUILayout.Button("Tools", GUILayout.Height(50))) page = 2;
            if (GUILayout.Button("Scene Tools", GUILayout.Height(50))) page = 3;
            GUILayout.EndHorizontal();
        }

        private void ViewToolsMenu() {
            TopPage("Tools");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Asset Usage Dectector", GUILayout.Height(50)))
                AssetUsageDetectorWindow.OpenActiveWindow();

            if (GUILayout.Button("Editor Icons", GUILayout.Height(50)))
                EditorIcons.EditorIconsOpen();

            if (GUILayout.Button("Bake All Scenes", GUILayout.Height(50)))
                BakeAllScenesWindow.ShowWindow();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(nameof(RequiredPrefab), GUILayout.Height(50)))
                RequiredPrefab.Instance.SelectInEditor();


            GUILayout.EndHorizontal();
        }

        private void ViewSceneTools() {
            TopPage("Scene Tools");
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Replace Gamobject", GUILayout.Height(50)))
                ReplaceGameObjects.CreateWizard();

            if (GUILayout.Button("Find Missing Scripts", GUILayout.Height(50)))
                FindMissingScripts.ShowWindow();

            GUILayout.EndHorizontal();
        }

        #endregion
    }
}