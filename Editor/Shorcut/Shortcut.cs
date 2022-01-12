#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ItsBaptiste.Toolbox.Editor.Shorcut {
    public static class Shortcut {
        public static void DisableAllScenes() {
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes) {
                SceneEnableExisting(scene.path, false);
            }
        }

        public static void EnableAllScenes() {
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes) {
                SceneEnableExisting(scene.path, true);
            }
        }

        public static void EnableScenesFromString(string value) {
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++) {
                bool actif = false;
                if (i < value.Length) actif = (value[i] == '1');
                // EditorBuildSettings.scenes[i].enabled = val;
                SceneEnableExisting(EditorBuildSettings.scenes[i].path, actif);
            }
        }

        /// <summary>
        ///  permet d'activer ou de desactiver une scene existant dans la liste du build settings
        /// </summary>
        /// <param name="sceneName">path ou nom de la scene</param>
        /// <param name="sceneEnabled">activer ou desactiver la scene dans le build</param>
        public static void SceneEnableExisting(string sceneName, bool sceneEnabled) {
            var editorBuildSettingsScenes = EditorBuildSettings.scenes;
            foreach (var scene in editorBuildSettingsScenes)
                if (scene.path.Contains(sceneName))
                    scene.enabled = sceneEnabled;

            EditorBuildSettings.scenes = editorBuildSettingsScenes;
        }


        /// <summary>
        /// supprimer une scene de la liste de la build
        /// </summary>
        /// <param name="path">chemin de la scene</param>
        public static void RemoveSceneToBuild(string path) {
            var original = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            var indexToRemove = -1;
            for (var i = 0; i < original.Count; i++)
                if (original[i].path == path) {
                    indexToRemove = i;
                    break;
                }

            if (indexToRemove != -1) {
                original.RemoveAt(indexToRemove);
                var newSettings = new EditorBuildSettingsScene[original.Count];
                Array.Copy(original.ToArray(), newSettings, original.Count);
                EditorBuildSettings.scenes = newSettings;
            }
        }

        /// <summary>
        ///     Ajoute la scéne dans la liste des scenes a builds et l'active si l'utilisateur le demande
        /// </summary>
        /// <param name="sceneNameOrPath">nom ou chemin de la scene a rajouter</param>
        /// <param name="sceneEnabled">active ou non la scene</param>
        public static void AddSceneToBuildList(string sceneNameOrPath, bool sceneEnabled) {
            var value = GetSceneStateInBuild(sceneNameOrPath);
            switch (value) {
                //ajoute la scéne au build
                case SceneBuildState.NotInList:
                    AddSceneToBuild(sceneNameOrPath);
                    break;
                case SceneBuildState.InList:
                case SceneBuildState.InListAndEnable:
                    Shortcut.SceneEnableExisting(sceneNameOrPath, sceneEnabled);
                    break;
            }
        }

        /// <summary>
        /// selectionne un asset a partir de son chemin 
        /// </summary>
        /// <param name="path">chemin complet de l'asset</param>
        public static void PingAssetByPath(string path) {
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath(path, typeof(Object)));
        }


        /// <summary>
        ///     ajoute la scene dans la liste du build settings et l'active si besoin
        /// </summary>
        /// <param name="path">chemin de la scene</param>
        /// <param name="sceneEnabled">definie si l'on doit activer la scene</param>
        private static void AddSceneToBuild(string path, bool sceneEnabled = true) {
            var original = EditorBuildSettings.scenes;
            var newSettings = new EditorBuildSettingsScene[original.Length + 1];
            Array.Copy(original, newSettings, original.Length);
            var sceneToAdd = new EditorBuildSettingsScene(path, sceneEnabled);
            newSettings[newSettings.Length - 1] = sceneToAdd;
            EditorBuildSettings.scenes = newSettings;
        }


        /// <summary>
        /// supprimer toutes les scenes de la liste de la build
        /// </summary>
        private static void RemoveAllScenes() {
            EditorBuildSettings.scenes = new EditorBuildSettingsScene[0];
        }

        /// <summary>
        ///     Retour le SceneBuildState de la scéne
        /// </summary>
        /// <param name="path">chemin de la scene</param>
        /// <returns>SceneBuildState</returns>
        private static SceneBuildState GetSceneStateInBuild(string path) {
            var scenes = EditorBuildSettings.scenes;
            foreach (var scene in scenes)
                if (scene.path.Contains(path)) {
                    if (scene.enabled) return SceneBuildState.InListAndEnable; //La scene est dans la liste mais pas activer
                    return SceneBuildState.InList; //La scene est activer
                }

            return SceneBuildState.NotInList; //La scene n'est pas dans la liste
        }

        public static string NameFromPath(string path) {
            var slash = path.LastIndexOf('/');
            var name = path.Substring(slash + 1);
            var dot = name.LastIndexOf('.');
            dot = Mathf.Max(0, dot); // wtf sans ça je ne peux pas build l'application meme si la class est entourée par un unity_editor Uu
            return name.Substring(0, dot);
        }

        private enum SceneBuildState {
            NotInList = -1,
            InList = 0,
            InListAndEnable = 1
        }
    }
}
#endif