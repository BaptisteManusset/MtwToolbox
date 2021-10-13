#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using ItsBaptiste;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

#pragma warning disable 414

public class ShortcutWindow : EditorWindow {
    private static readonly List<EditorBuildSettingsScene> ScenesInBuildList = new List<EditorBuildSettingsScene>();
    private int _tab;
    private static int _scriptableSelected = 0;

    private static Vector2 _scrollPosition = Vector2.zero;

    [MenuItem("Tools/Shortcut", false, 1)]
    public static void ShowWindow() {
        ShortcutWindow window = (ShortcutWindow)GetWindow(typeof(ShortcutWindow));
        window.name = "Shortcut";
        window.Show();
        ReloadSceneinBuildList();
    }

    private static void ReloadSceneinBuildList() {
        ScenesInBuildList.Clear();
        ScenesInBuildList.AddRange(EditorBuildSettings.scenes.OrderBy(x => x.path));
    }

    private static string GetSubFolder(string path) {
        path = path.Replace("Assets/Scenes/", ""); // supprime "Assets/Scenes/" du chemin de la scenes puis récupere le texte a droite du "/" donc le dossier suivant
        path = path.Split('/')[0];
        if (path.Contains(".unity")) path = "";
        return path;
    }

    private void OnGUI() {
        if (EditorApplication.isPlaying) {
            ViewOnPlay();
            return;
        }

        View();
    }

    private void View() {
        ReloadSceneinBuildList();
        ViewListOfAllLoadedScenesInEditor();
        ViewListOfAllScenesInBuildList();
    }

    private void ToolBar() {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Scene dans la build");
        if (MtwStyle.ButtonIcon("d_Project", "Ouvrir le dossier scenes", GUILayout.Width(30)))
            EditorUtility.RevealInFinder(EditorBuildSettings.scenes[0].path);


        if (GUILayout.Button("<b>All</b>", MtwStyle.StyleButton)) {
            Shortcut.EnableAllScenes();
            ReloadSceneinBuildList();
        }

        if (GUILayout.Button("<b>None</b>", MtwStyle.StyleButton)) {
            Shortcut.DisableAllScenes();
            ReloadSceneinBuildList();
        }

        GUILayout.EndHorizontal();
    }

    private void ViewListOfAllScenesInBuildList() {
        if (ScenesInBuildList.Count > 0) {
            ToolBar();

            // liste des scenes dispo

            GUILayout.BeginVertical(EditorStyles.helpBox);
            _scrollViewPosition = EditorGUILayout.BeginScrollView(_scrollViewPosition);
            foreach (EditorBuildSettingsScene t in ScenesInBuildList) ViewDisplayLoadedScene(t);

            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
        else {
            //no scene found
            ViewNoScenesFoundPleaseReload();
        }
    }

    private void ViewListOfAllLoadedScenesInEditor() {
        GUILayout.Label("Loaded", EditorStyles.largeLabel);
        GUILayout.BeginVertical(EditorStyles.helpBox);
        _scrollViewPositionEditor = EditorGUILayout.BeginScrollView(_scrollViewPositionEditor,
            GUILayout.MinHeight(SceneManager.sceneCount * 23),
            GUILayout.MaxHeight(position.height / 3));

        for (int i = 0; i < SceneManager.sceneCount; i++) {
            Scene scene = SceneManager.GetSceneAt(i);
            GUILayout.BeginHorizontal();

            if (scene.buildIndex == -1)
                if (MtwStyle.ButtonIcon("CollabPush", "Activer la scene dans la builds")) {
                    Shortcut.AddSceneToBuildList(scene.path, true);
                    ReloadSceneinBuildList();
                    ShowWindow();
                }

            if (MtwStyle.ButtonIcon("d_AvatarCompass")) Shortcut.PingAssetByPath(scene.path);
            DisplayName(scene.path);
            GUILayout.FlexibleSpace();
            ButtonCloseScene(scene);
            GUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        GUILayout.EndVertical();
    }

    private static void ButtonCloseScene(Scene scene) {
        GUI.backgroundColor = Color.red;
        if (MtwStyle.ButtonIcon("d_winbtn_win_close", "Unload")) EditorSceneManager.CloseScene(scene, true);

        GUI.backgroundColor = Color.white;
    }


    private void ViewDisplayLoadedScene(EditorBuildSettingsScene scene) {
        GUILayout.BeginHorizontal();

        //change l'etat de la scene entre actif et desactiver & change le voyant

        ButtonToggleSceneInBuild(scene);
        ButtonPingAsset(scene.path);
        Seperator();
        ButtonReplaceScene(scene.path);
        ButtonOpenSceneAdditive(scene.path);
        Seperator();

        DisplayName(scene.path);
        GUILayout.FlexibleSpace();
        ButtonRemoveSceneFromBuild(scene);

        GUILayout.EndHorizontal();
    }

    private static void Seperator() {
        GUILayout.Label("", GUILayout.Width(0));
    }


    private static void ViewNoScenesFoundPleaseReload() {
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


    private static void ButtonOpenSceneAdditive(string path) {
        if (MtwStyle.ButtonIcon("CreateAddNew", "Add Scene")) EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
    }

    private static void ButtonReplaceScene(string path) {
        if (MtwStyle.ButtonIcon("d_UnityEditor.Graphs.AnimatorControllerTool", "Replace Actual Scene"))
            if (EditorUtility.DisplayDialog("Replace the scene", "Replace actual scene and load that scene ?", "Yes", "Cancel")) {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                EditorSceneManager.OpenScene(path);
            }
    }


    private void ButtonRemoveSceneFromBuild(EditorBuildSettingsScene scene) {
        if (MtwStyle.ButtonIcon("d_TreeEditor.Trash", "Remove the Scene from the build"))
            if (EditorUtility.DisplayDialog(
                "Remove the Scene from the build",
                $"Removing {scene.path} from the build ?", "Yes remove", "Cancel")) {
                Shortcut.RemoveSceneToBuild(scene.path);
                ReloadSceneinBuildList();
            }
    }

    private void ButtonToggleSceneInBuild(EditorBuildSettingsScene scene) {
        if (MtwStyle.ButtonIcon(scene.enabled ? "d_winbtn_mac_max" : "d_winbtn_mac_close", scene.enabled ? "Desactiver la scene dans la build" : "Activer la scene dans la build")) {
            Shortcut.AddSceneToBuildList(scene.path, !scene.enabled);
            ShowWindow();
        }
    }

    /// <summary>
    ///     selection le fichier de la scéne
    /// </summary>
    /// <param name="path"></param>
    private static void ButtonPingAsset(string path) {
        if (MtwStyle.ButtonIcon("d_AvatarCompass")) Shortcut.PingAssetByPath(path);
    }


    private Vector2 _scrollViewPosition;
    private Vector2 _scrollViewPositionEditor;


    //display all Layer for the camera
    private void EnableTrueVision() {
        Camera[] cams = FindObjectsOfType<Camera>();
        foreach (Camera cam in cams) cam.cullingMask = -1;
    }


    public static void DisplayName(string path) {
        MtwStyle.Label(path, GetSubFolder(path));
        if (GUILayout.Button(Shortcut.NameFromPath(path), EditorStyles.boldLabel)) Shortcut.PingAssetByPath(path);

        GUILayout.FlexibleSpace();
    }
}
#endif