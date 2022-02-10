using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ItsBaptiste.Toolbox.BakeAllScenesWindow.Editor {
    public class BakeAllScenesWindow : EditorWindow {
        private int _numberOfScenes;
        private SceneAsset[] _scenes = new SceneAsset[0];
        private string _scenesLocation = "";

        private void OnGUI() {
            GUILayout.Label("Select scenes manually", EditorStyles.boldLabel);

            _numberOfScenes = EditorGUILayout.IntField("Number of scenes", _numberOfScenes);
            if (_numberOfScenes > _scenes.Length) {
                // Number of scenes has increased, copy the current ones and leave space for new ones
                SceneAsset[] copy = (SceneAsset[])_scenes.Clone();
                _scenes = new SceneAsset[_numberOfScenes];
                for (int i = 0; i < copy.Length; i++) _scenes[i] = copy[i];
            }
            else if (_numberOfScenes < _scenes.Length) {
                // Number of scenes has decreased, remove the last ones
                SceneAsset[] copy = (SceneAsset[])_scenes.Clone();
                _scenes = new SceneAsset[_numberOfScenes];
                for (int i = 0; i < _scenes.Length; i++) _scenes[i] = copy[i];
            }

            // if numberOfScenes equals scenes.Lenght, nothing has changed
            for (int i = 0; i < _scenes.Length; i++)
                _scenes[i] = (SceneAsset)EditorGUILayout.ObjectField(_scenes[i], typeof(SceneAsset), false);

            GUILayout.Space(15);
            GUILayout.Label("Include all scenes in this folder", EditorStyles.boldLabel);
            _scenesLocation = EditorGUILayout.TextField("Scenes folder", _scenesLocation);
            GUILayout.Label("(for example: Assets/Scenes/", EditorStyles.miniLabel);

            GUILayout.Space(20);
            if (GUILayout.Button("Start baking")) {
                bool startBake = true;
                List<string> pathsToBake = new List<string>();
                if (!string.IsNullOrEmpty(_scenesLocation))
                    try {
                        if (!_scenesLocation.Contains("Assets"))
                            _scenesLocation = (_scenesLocation[0] == '/' ? "Assets" : "Assets/") + _scenesLocation;

                        string[] files = Directory.GetFiles(_scenesLocation);
                        foreach (string s in files)
                            if (s.Contains(".unity") && !s.Contains(".meta"))
                                pathsToBake.Add(s);
                    }
                    catch (DirectoryNotFoundException) {
                        Debug.LogError(string.Format("Cannot find directory: {0}", _scenesLocation));
                        startBake = false;
                    }

                try {
                    for (int i = 0; i < _scenes.Length; i++)
                        if (_scenes[i] != null) {
                            string path = AssetDatabase.GetAssetPath(_scenes[i]);
                            if (pathsToBake.Contains(path))
                                Debug.Log(string.Format("{0} is already added", _scenes[i]));
                            else
                                pathsToBake.Add(path);
                        }
                }
                catch (Exception ex) {
                    Debug.LogError(string.Format("Unexpected error: {0}", ex.Message));
                    startBake = false;
                }

                if (startBake) BakeScenes(pathsToBake.ToArray());
            }
        }

        [MenuItem("Tools/AssetStore/Bake multiple scenes")]
        public static void ShowWindow() {
            BakeAllScenesWindow window = (BakeAllScenesWindow)GetWindow(typeof(BakeAllScenesWindow));
            window.titleContent.text = "Multi-scene baking";
            window.Show();
        }

        private void BakeScenes(string[] toBake) {
            foreach (string scenePath in toBake) {
                Debug.Log(string.Format("Starting: {0}", scenePath));
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                if (Lightmapping.Bake())
                    Debug.Log(string.Format("Bake success: {0}", scenePath));
                else
                    Debug.LogError(string.Format("Error baking: {0}", scenePath));

                EditorSceneManager.SaveOpenScenes();
            }
        }
    }
}