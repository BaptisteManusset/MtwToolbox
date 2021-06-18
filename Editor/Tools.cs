﻿using AssetUsageDetectorNamespace;
using UnityEditor;
using UnityEngine;

namespace Editor {
    public class Tools : EditorWindow {
        static int page = 0;


        [MenuItem("Tools/Tools")]
        private static void ShowWindow() {
            var window = GetWindow<Tools>();
            window.titleContent = new GUIContent("Tools");
            window.Show();
        }

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

        #region Views

        private void ViewScene() {
            TopPage("Scene");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Load Scenes", GUILayout.Height(50))) SceneLink.ChangeStateToPlay();
            if (GUILayout.Button("Load Scenes", GUILayout.Height(50))) SceneLink.ChangeStateToMenu();
            GUILayout.EndHorizontal();

            // if (GUILayout.Button("Unload Scenes", GUILayout.Height(50))) SceneLink.UnloadScenes();
            if (GUILayout.Button("Display Asset", GUILayout.Height(50))) SceneLink.Instance.PingInEditor();
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
        }

        private void ViewSceneTools() {
            TopPage("Scene Tools");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Replace Gamobject", GUILayout.Height(50)))
                ReplaceGameObjects.CreateWizard();
            GUILayout.EndHorizontal();
        }

        #endregion

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
    }
}