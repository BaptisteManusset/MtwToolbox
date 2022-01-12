using ArdenfallEditor.Utility;
using AssetUsageDetectorNamespace;
using ItsBaptiste.Core;
using ItsBaptiste.GameEvent.Editor;
using ItsBaptiste.Inventory.Core;
using ItsBaptiste.LazyGun;
using ItsBaptiste.Scriptabbles.RequiredPrefab;
using ItsBaptiste.Scriptabbles.SceneLink;
using ItsBaptiste.Toolbox.Editor.Shorcut;
using ItsBaptiste.Windows;
using Toolbox.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ItsBaptiste.Toolbox.Editor {
    public class HubTools : EditorWindow {
        private int _page = 0;

        private void OnGUI() {
            switch (_page) {
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


        [MenuItem("Tools/Tools", priority = -1000)]
        private static void ShowWindow() {
            HubTools window = GetWindow<HubTools>();
            window.titleContent = new GUIContent(nameof(HubTools));
            window.Show();
        }

        [MenuItem("🌐 𝗘𝗫𝗧𝗘𝗥𝗡𝗔𝗟/Notion")]
        private static void OpenNotion() => Application.OpenURL("https://www.notion.so/itsbaptiste/Gamedev-84e8cb13359c48d9be7d8662591e9bc8");

        [MenuItem("🌐 𝗘𝗫𝗧𝗘𝗥𝗡𝗔𝗟/Trello")]
        private static void OpenTrello() => Application.OpenURL("https://trello.com/b/X2LKhO8w/unity");


        [MenuItem("🌐 𝗘𝗫𝗧𝗘𝗥𝗡𝗔𝗟/Inspiration")]
        private static void OpenInspiration() => Application.OpenURL("https://drive.google.com/drive/u/1/folders/1hBiQjx4Kr3m9LdXd6tDmEmHDpkZWmLQm");

        [MenuItem("🌐 𝗘𝗫𝗧𝗘𝗥𝗡𝗔𝗟/Git")]
        private static void OpenGit() => Application.OpenURL("https://github.com/BaptisteManusset/ffffffps");


        #region Editor

        private void TopPage(string pageName = "") {
            GUILayout.BeginHorizontal(GUI.skin.box);
            if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("tab_prev@2x")), GUILayout.Height(30), GUILayout.Width(30))) _page = 0;
            GUILayout.FlexibleSpace();
            GUILayout.Label(pageName, GUILayout.Height(30));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        #endregion

        #region Views

        private void ViewScene() {
            TopPage("Scene");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Load Menu", GUILayout.Height(50))) {
                EditorSceneManager.OpenScene(SceneLink.Instance.menu);
                EditorSceneManager.SetActiveScene(EditorSceneManager.GetSceneByPath(SceneLink.Instance.menu));

                EditorSceneManager.OpenScene(SceneLink.Instance.common, OpenSceneMode.Additive);
            }


            if (GUILayout.Button("Load Game", GUILayout.Height(50))) {
                EditorSceneManager.OpenScene(SceneLink.Instance.Level);
                EditorSceneManager.SetActiveScene(EditorSceneManager.GetSceneByPath(SceneLink.Instance.Level));

                EditorSceneManager.OpenScene(SceneLink.Instance.Level);
                EditorSceneManager.SetActiveScene(EditorSceneManager.GetSceneByPath(SceneLink.Instance.Level));

                EditorSceneManager.OpenScene(SceneLink.Instance.ingameControl, OpenSceneMode.Additive);
                EditorSceneManager.OpenScene(SceneLink.Instance.hud, OpenSceneMode.Additive);
                EditorSceneManager.OpenScene(SceneLink.Instance.player, OpenSceneMode.Additive);
                EditorSceneManager.OpenScene(SceneLink.Instance.common, OpenSceneMode.Additive);
            }


            GUILayout.EndHorizontal();

            // if (GUILayout.Button("Unload Scenes", GUILayout.Height(50))) SceneLink.UnloadScenes();
            if (GUILayout.Button("Display Asset", GUILayout.Height(50))) SceneLink.Instance.SelectInEditor();
        }


        private void ViewMain() {
            GUILayout.BeginHorizontal(GUI.skin.box);
            if (GUILayout.Button("Scenes", GUILayout.Height(50))) _page = 1;
            if (GUILayout.Button("Tools", GUILayout.Height(50))) _page = 2;
            if (GUILayout.Button("Scene Tools", GUILayout.Height(50))) _page = 3;
            GUILayout.EndHorizontal();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUILayout.Label("Rapide :");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Shortcut", GUILayout.Height(50))) ShortcutWindow.ShowWindow();
            if (GUILayout.Button("Game Events", GUILayout.Height(50))) GameEventList.ShowWindow();
            if (GUILayout.Button("Asset Library", GUILayout.Height(50))) AssetLibraryWindow.OpenWindow();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Fbx 2 Prefab", GUILayout.Height(50))) Fbx2PrefabWindow.ShowWindow();
            if (GUILayout.Button("Scene approval", GUILayout.Height(50))) SceneApproval.ShowWindow();
            GUILayout.EndHorizontal();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Game Variable", GUILayout.Height(50))) FindAssetAndOpenInInspector(nameof(GameVariable));
            if (GUILayout.Button("Items object", GUILayout.Height(50))) FindAssetAndOpenInInspector(nameof(Item));
            if (GUILayout.Button("Gun Stat", GUILayout.Height(50))) FindAssetAndOpenInInspector(nameof(GunStat));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(nameof(InputConfiguration), GUILayout.Height(50))) {
                string[] objsGuid = AssetDatabase.FindAssets(nameof(InputConfiguration));

                Shortcut.PingAssetByPath(AssetDatabase.GUIDToAssetPath(objsGuid[1]));

                AssetDatabase.LoadAssetAtPath<GameVariable>(AssetDatabase.GUIDToAssetPath(objsGuid[1])).PingInEditor();
            }

            GUILayout.EndHorizontal();
        }

        private void FindAssetAndOpenInInspector(string name) {
            string[] objsGuid = AssetDatabase.FindAssets("t:" + name);

            Shortcut.PingAssetByPath(AssetDatabase.GUIDToAssetPath(objsGuid[0]));

            AssetDatabase.LoadAssetAtPath<GameVariable>(AssetDatabase.GUIDToAssetPath(objsGuid[0])).PingInEditor();
            // for (int i = 0; i < count; i++) objs[i] = AssetDatabase.LoadAssetAtPath<GameVariable>(AssetDatabase.GUIDToAssetPath(objsGuid[i]));
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

            GUILayout.BeginHorizontal();
            //
            // if (GUILayout.Button("Scene Initializer", GUILayout.Height(50))) {
            //     ConfigurationScene();
            // }

            GUILayout.EndHorizontal();
        }

        // private static void ConfigurationScene() {
        //     string[] names = new[] { "Post process", "Ennemies", "Decor Statique", "Decor Dynamic", "Technique", "Interactable" };
        //
        //     foreach (string n in names) {
        //         FolderEditorUtils.AddFolderPrefab(n);
        //     }
        // }

        #endregion
    }
}