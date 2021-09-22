#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AssetUsageDetectorNamespace;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

#pragma warning disable 414

public class Shortcut : EditorWindow {
    #region Variables

    #region Window

    private static readonly List<EditorBuildSettingsScene> ScenesInBuildList = new List<EditorBuildSettingsScene>();
    private int _tab;
    private static string _scriptableName = "";
    private static int _scriptableSelected = 0;
    private static Vector2 _scrollPosition = Vector2.zero;
    private static readonly GUIStyle StyleTxt = new GUIStyle();
    private static GUIStyle _styleButton = new GUIStyle();
    private static GUIStyle _styleFolder = new GUIStyle();

    #endregion

    #endregion

    #region INIT

    // [DidReloadScripts]
    [MenuItem("Tools/Shortcut", false, 1)]
    public static void ShowWindow() {
        var window = (Shortcut)GetWindow(typeof(Shortcut));
        window.Show();
        ReloadSceneinBuildList();
    }

    private static void ReloadSceneinBuildList() {
        ScenesInBuildList.Clear();
        ScenesInBuildList.AddRange(EditorBuildSettings.scenes.OrderBy(x => x.path));
    }

    private static string GetSubFolder(string path) {
        path = path.Remove(0, 14); // supprime "Assets/Scenes/" du chemin de la scenes puis récupere le texte a droite du "/" donc le dossier suivant
        path = path.Split('/')[0];
        if (path.Contains(".unity")) path = "";
        return path;
    }

    private void OnGUI() {
        StyleTxt.richText = true;
        _styleButton = new GUIStyle(GUI.skin.button) {
            richText = true
        };
        _styleFolder = new GUIStyle(GUI.skin.label) {
            richText = true,
            fontSize = 10,
            alignment = TextAnchor.MiddleRight
        };
        if (EditorApplication.isPlaying) {
            ViewOnPlay();
            return;
        }

        ViewListOfAllSceneInBuild();
    }

    #endregion

    #region views

    bool showPosition = true;

    private void ViewListOfAllSceneInBuild() {
        ReloadSceneinBuildList();
        GUILayout.BeginHorizontal();

        if (GUILayout.Button(
            new GUIContent(
                EditorGUIUtility.FindTexture("d_Project"),
                "Ouvrir le dossier scenes"), GUILayout.Width(30))) {
            EditorUtility.RevealInFinder(EditorBuildSettings.scenes[0].path);
        }

        if (GUILayout.Button("<b>All</b>", _styleButton)) EnableAllScenes();
        if (GUILayout.Button("<b>None</b>", _styleButton)) DisableAllScenes();
        GUILayout.EndHorizontal();

        if (ScenesInBuildList.Count > 0) {
            // liste des scenes dispo

            showPosition = EditorGUILayout.Foldout(showPosition, "Scene dans la build");

            if (showPosition)
                foreach (EditorBuildSettingsScene t in ScenesInBuildList) {
                    ViewDisplayLoadedScene(t);
                }


            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }
        else {
            //no scene found
            NoScenesFoundPleaseReload();
        }

        GUILayout.Label("Loaded", EditorStyles.largeLabel);

        for (int i = 0; i < EditorSceneManager.sceneCount; i++) {
            Scene scene = EditorSceneManager.GetSceneAt(i);

            GUILayout.BeginHorizontal();


            if (GUILayout.Button(new GUIContent(EditorGUIUtility.FindTexture("CollabPush"), "Activer la scene dans la builds"))) {
                AddSceneToBuildList(scene.path, true);
                ShowWindow();
            }

            GUILayout.Label(scene.name);
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button(new GUIContent(EditorGUIUtility.FindTexture("d_winbtn_win_close"), "Unload"))) {
                EditorSceneManager.CloseScene(scene, true);
            }

            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();
        }


        GUILayout.Space(30);
    }


    private void DisableAllScenes() {
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes) {
            SceneEnableExisting(scene.path, false);
        }

        ReloadSceneinBuildList();
    }

    private void EnableAllScenes() {
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes) {
            SceneEnableExisting(scene.path, true);
        }

        ReloadSceneinBuildList();
    }

    private void EnableScenesFromString(string value) {
        for (int i = 0; i < EditorBuildSettings.scenes.Length; i++) {
            bool actif = false;
            if (i < value.Length) actif = (value[i] == '1');
            // EditorBuildSettings.scenes[i].enabled = val;
            SceneEnableExisting(EditorBuildSettings.scenes[i].path, actif);
        }
    }


    private void ViewDisplayLoadedScene(EditorBuildSettingsScene item) {
        GUILayout.BeginHorizontal();

        //change l'etat de la scene entre actif et desactiver & change le voyant
        if (GUILayout.Button(
            new GUIContent(
                EditorGUIUtility.FindTexture(item.enabled ? "d_winbtn_mac_max" : "d_winbtn_mac_close"),
                item.enabled ? "Desactiver la scene dans la builds" : "Activer la scene dans la builds")
        )) {
            AddSceneToBuildList(item.path, !item.enabled);
            ShowWindow();
        }

        //selection le fichier de la scéne
        if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("d_AvatarCompass")))) PingAssetByPath(item.path);

        SceneButtons(item.path);
        DisplayNameAndPath(item.path);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(
            new GUIContent(
                EditorGUIUtility.FindTexture("d_TreeEditor.Trash"),
                "Remove the Scene from the build")))
            if (EditorUtility.DisplayDialog(
                "Remove the Scene from the build",
                $"Removing {item.path} from the build ?", "Yes remove", "Cancel")) {
                RemoveSceneToBuild(item.path);
                ReloadSceneinBuildList();
            }

        GUILayout.EndHorizontal();
    }

    private static void NoScenesFoundPleaseReload() {
        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Space(60);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Aucune scene disponible");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(30);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Reload")) ReloadSceneinBuildList();

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(60);
        GUILayout.EndVertical();
    }

    private void ViewOnPlay() { }

    #endregion


    #region Buttons

    private static void SceneButtons(string path) {
        GUILayout.BeginHorizontal(EditorStyles.helpBox);
        if (GUILayout.Button(new GUIContent(EditorGUIUtility.FindTexture("d_UnityEditor.Graphs.AnimatorControllerTool"), "Replace Actual Scene"))) {
            if (EditorUtility.DisplayDialog("Replace the scene", "Replace actual scene and load that scene ?", "Yes", "Cancel")) {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                EditorSceneManager.OpenScene(path);
            }
        }

        if (GUILayout.Button(new GUIContent(EditorGUIUtility.FindTexture("CreateAddNew"), "Add Scene"))) {
            EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
        }

        GUILayout.EndHorizontal();
    }

    #endregion


    string _nameMask = "";


    /// <summary>
    /// selectionne un asset a partir de son chemin 
    /// </summary>
    /// <param name="path">chemin complet de l'asset</param>
    public static void PingAssetByPath(string path) {
        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath(path, typeof(Object)));
    }

    /// <summary>
    ///     Ajoute la scéne dans la liste des scenes a builds et l'active si l'utilisateur le demande
    /// </summary>
    /// <param name="sceneNameOrPath">nom ou chemin de la scene a rajouter</param>
    /// <param name="sceneEnabled">active ou non la scene</param>
    private void AddSceneToBuildList(string sceneNameOrPath, bool sceneEnabled) {
        var value = GetSceneStateInBuild(sceneNameOrPath);
        switch (value) {
            //ajoute la scéne au build
            case SceneBuildState.NotInList:
                AddSceneToBuild(sceneNameOrPath);
                break;
            case SceneBuildState.InList:
            case SceneBuildState.InListAndEnable:
                SceneEnableExisting(sceneNameOrPath, sceneEnabled);
                break;
        }
    }

    /// <summary>
    ///  permet d'activer ou de desactiver une scene existant dans la liste du build settings
    /// </summary>
    /// <param name="sceneName">path ou nom de la scene</param>
    /// <param name="sceneEnabled">activer ou desactiver la scene dans le build</param>
    private void SceneEnableExisting(string sceneName, bool sceneEnabled) {
        var editorBuildSettingsScenes = EditorBuildSettings.scenes;
        foreach (var scene in editorBuildSettingsScenes)
            if (scene.path.Contains(sceneName))
                scene.enabled = sceneEnabled;

        EditorBuildSettings.scenes = editorBuildSettingsScenes;
    }

    /// <summary>
    ///     ajoute la scene dans la liste du build settings et l'active si besoin
    /// </summary>
    /// <param name="path">chemin de la scene</param>
    /// <param name="sceneEnabled">definie si l'on doit activer la scene</param>
    private void AddSceneToBuild(string path, bool sceneEnabled = true) {
        var original = EditorBuildSettings.scenes;
        var newSettings = new EditorBuildSettingsScene[original.Length + 1];
        Array.Copy(original, newSettings, original.Length);
        var sceneToAdd = new EditorBuildSettingsScene(path, sceneEnabled);
        newSettings[newSettings.Length - 1] = sceneToAdd;
        EditorBuildSettings.scenes = newSettings;
    }

    /// <summary>
    /// supprimer une scene de la liste de la build
    /// </summary>
    /// <param name="path">chemin de la scene</param>
    private void RemoveSceneToBuild(string path) {
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
            ReloadSceneinBuildList();
        }
    }

    /// <summary>
    /// supprimer toutes les scenes de la liste de la build
    /// </summary>
    private void RemoveAllScenes() {
        EditorBuildSettings.scenes = new EditorBuildSettingsScene[0];
    }

    /// <summary>
    ///     Retour le SceneBuildState de la scéne
    /// </summary>
    /// <param name="path">chemin de la scene</param>
    /// <returns>SceneBuildState</returns>
    private SceneBuildState GetSceneStateInBuild(string path) {
        var scenes = EditorBuildSettings.scenes;
        foreach (var scene in scenes)
            if (scene.path.Contains(path)) {
                if (scene.enabled) return SceneBuildState.InListAndEnable; //La scene est dans la liste mais pas activer
                return SceneBuildState.InList; //La scene est activer
            }

        return SceneBuildState.NotInList; //La scene n'est pas dans la liste
    }

    public static string NameFromPath(string path) {
        //string path = SceneUtility.GetScenePathByBuildIndex(BuildIndex);
        var slash = path.LastIndexOf('/');
        var name = path.Substring(slash + 1);
        var dot = name.LastIndexOf('.');
        return name.Substring(0, dot);
    }

    private enum SceneBuildState {
        NotInList = -1,
        InList = 0,
        InListAndEnable = 1
    }


    #region cheat

    //display all Layer for the camera
    private void EnableTrueVision() {
        Camera[] cams = FindObjectsOfType<Camera>();
        foreach (Camera cam in cams) {
            cam.cullingMask = -1;
        }
    }

    #endregion

    #region Utilitys

    public static void DisplayNameAndPath(string path, bool displayPath = false) {
        float h = Mathf.Abs((float)(GetSubFolder(path).GetHashCode() % 10)) / 10;
        GUI.backgroundColor = Color.HSVToRGB(h, 1, 1);
        GUILayout.Label(GetSubFolder(path), EditorStyles.miniButton);
        GUI.backgroundColor = Color.white;
        // GUILayout.Label($"{NameFromPath(path)}", EditorStyles.boldLabel);
        if (GUILayout.Button(NameFromPath(path), EditorStyles.boldLabel)) PingAssetByPath(path);

        GUILayout.FlexibleSpace();

        if (displayPath) {
            string text = path;
            if (GUILayout.Button(
                $"{text}",
                EditorStyles.centeredGreyMiniLabel)) {
                PingAssetByPath(path);
                ReloadSceneinBuildList();
            }
        }
    }

    #endregion
}
//
//
// /// <summary>
// /// Scene auto loader.
// /// </summary>
// /// <description>
// /// This class adds a File > Scene Autoload menu containing options to select
// /// a "master scene" enable it to be auto-loaded when the user presses play
// /// in the editor. When enabled, the selected scene will be loaded on play,
// /// then the original scene will be reloaded on stop.
// ///
// /// Based on an idea on this thread:
// /// http://forum.unity3d.com/threads/157502-Executing-first-scene-in-build-settings-when-pressing-play-button-in-editor
// /// </description>
// [InitializeOnLoad]
// static class SceneAutoLoader {
//     // Static constructor binds a playmode-changed callback.
//     // [InitializeOnLoad] above makes sure this gets executed.
//     static SceneAutoLoader() {
//         EditorApplication.playModeStateChanged += OnPlayModeChanged;
//     }
//
//     // Menu items to select the "master" scene and control whether or not to load it.
//     [MenuItem("File/Scene Autoload/Select Master Scene...")]
//     private static void SelectMasterScene() {
//         string masterScene = EditorUtility.OpenFilePanel("Select Master Scene", Application.dataPath, "unity");
//         masterScene = masterScene.Replace(Application.dataPath, "Assets"); //project relative instead of absolute path
//         if (!string.IsNullOrEmpty(masterScene)) {
//             MasterScene = masterScene;
//             LoadMasterOnPlay = true;
//         }
//     }
//
//     [MenuItem("File/Scene Autoload/Load Master On Play", true)]
//     private static bool ShowLoadMasterOnPlay() {
//         return !LoadMasterOnPlay;
//     }
//
//     [MenuItem("File/Scene Autoload/Load Master On Play")]
//     private static void EnableLoadMasterOnPlay() {
//         LoadMasterOnPlay = true;
//     }
//
//     [MenuItem("File/Scene Autoload/Don't Load Master On Play", true)]
//     private static bool ShowDontLoadMasterOnPlay() {
//         return LoadMasterOnPlay;
//     }
//
//     [MenuItem("File/Scene Autoload/Don't Load Master On Play")]
//     private static void DisableLoadMasterOnPlay() {
//         LoadMasterOnPlay = false;
//     }
//
//     // Play mode change callback handles the scene load/reload.
//     private static void OnPlayModeChanged(PlayModeStateChange state) {
//         if (!LoadMasterOnPlay) {
//             return;
//         }
//
//
//         if (!EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode) {
//             // User pressed play -- autoload master scene.
//             PreviousScene = EditorSceneManager.GetActiveScene().path;
//
//
//             //if the scene is not in scenes build list cancel
//             if (SceneUtility.GetBuildIndexByScenePath(PreviousScene) == -1) return;
//
//             if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
//                 try {
//                     EditorSceneManager.OpenScene(MasterScene);
//                 }
//                 catch {
//                     Debug.LogError(string.Format("error: scene not found: {0}", MasterScene));
//                     EditorApplication.isPlaying = false;
//                 }
//             }
//             else {
//                 // User cancelled the save operation -- cancel play as well.
//                 EditorApplication.isPlaying = false;
//             }
//         }
//
//         // isPlaying check required because cannot OpenScene while playing
//         if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode) {
//             // User pressed stop -- reload previous scene.
//             try {
//                 EditorSceneManager.OpenScene(PreviousScene);
//             }
//             catch {
//                 Debug.LogError(string.Format("error: scene not found: {0}", PreviousScene));
//             }
//         }
//     }
//
//     // Properties are remembered as editor preferences.
//     private const string CEditorPrefLoadMasterOnPlay = "SceneAutoLoader.LoadMasterOnPlay";
//     private const string CEditorPrefMasterScene = "SceneAutoLoader.MasterScene";
//     private const string CEditorPrefPreviousScene = "SceneAutoLoader.PreviousScene";
//
//     public static bool LoadMasterOnPlay {
//         get { return EditorPrefs.GetBool(CEditorPrefLoadMasterOnPlay, false); }
//         set { EditorPrefs.SetBool(CEditorPrefLoadMasterOnPlay, value); }
//     }
//
//     private static string MasterScene {
//         get { return EditorPrefs.GetString(CEditorPrefMasterScene, "Master.unity"); }
//         set { EditorPrefs.SetString(CEditorPrefMasterScene, value); }
//     }
//
//     private static string PreviousScene {
//         get { return EditorPrefs.GetString(CEditorPrefPreviousScene, EditorSceneManager.GetActiveScene().path); }
//         set { EditorPrefs.SetString(CEditorPrefPreviousScene, value); }
//     }
// }
#endif