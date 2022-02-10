#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using ItsBaptiste.SceneLink.Core;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

namespace ItsBaptiste.Toolbox.SceneReference {
    /// <summary>
    /// Display a Scene Reference object in the editor.
    /// If scene is valid, provides basic buttons to interact with the scene's role in Build Settings.
    /// </summary>
    [CustomPropertyDrawer(typeof(SceneReference))]
    public class SceneReferencePropertyDrawer : PropertyDrawer {
        // The exact name of the asset Object variable in the SceneReference object
        private const string SceneAssetPropertyString = "sceneAsset";

        // The exact name of the scene Path variable in the SceneReference object
        private const string ScenePathPropertyString = "scenePath";

        private static readonly RectOffset BoxPadding = EditorStyles.helpBox.padding;


        // Made these two const btw
        private const float PadSize = 2f;
        private const float FooterHeight = 10f;

        private static readonly float LineHeight = EditorGUIUtility.singleLineHeight;
        private static readonly float PaddedLine = LineHeight + PadSize;

        /// <summary>
        /// Drawing the 'SceneReference' property
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            // Move this up
            EditorGUI.BeginProperty(position, GUIContent.none, property);
            // {
            // Here we add the foldout using a single line height, the label and change
            // the value of property.isExpanded
            EditorGUI.PrefixLabel(new Rect(position.x, position.y, position.width, LineHeight), label);
            // property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, lineHeight), property.isExpanded, label);

            // Now you want to draw the content only if you unfold this property
            // if (property.isExpanded) {
            // Optional: Indent the content
            //EditorGUI.indentLevel++;
            //{

            // reduce the height by one line and move the content one line below
            position.height -= LineHeight;
            position.y += LineHeight;

            SerializedProperty sceneAssetProperty = GetSceneAssetProperty(property);

            // Draw the Box Background
            position.height -= FooterHeight;
            GUI.Box(EditorGUI.IndentedRect(position), GUIContent.none, EditorStyles.helpBox);
            position = BoxPadding.Remove(position);
            position.height = LineHeight;

            // Draw the main Object field
            label.tooltip = "The actual Scene Asset reference.\nOn serialize this is also stored as the asset's path.";


            int sceneControlID = GUIUtility.GetControlID(FocusType.Passive);
            EditorGUI.BeginChangeCheck();
            {
                // removed the label here since we already have it in the foldout before
                sceneAssetProperty.objectReferenceValue = EditorGUI.ObjectField(position, sceneAssetProperty.objectReferenceValue, typeof(SceneAsset), false);
            }
            BuildUtils.BuildScene buildScene = BuildUtils.GetBuildScene(sceneAssetProperty.objectReferenceValue);
            if (EditorGUI.EndChangeCheck()) {
                // If no valid scene asset was selected, reset the stored path accordingly
                if (buildScene.Scene == null) GetScenePathProperty(property).stringValue = string.Empty;
            }

            position.y += PaddedLine;

            if (!buildScene.AssetGuid.Empty()) {
                // Draw the Build Settings Info of the selected Scene
                DrawSceneInfoGUI(position, buildScene, sceneControlID + 1);
            }

            // Optional: If enabled before reset the indentlevel
            //}
            //EditorGUI.indentLevel--;
            // }
            // }
            EditorGUI.EndProperty();
        }

        /// <summary>
        /// Ensure that what we draw in OnGUI always has the room it needs
        /// </summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            SerializedProperty sceneAssetProperty = GetSceneAssetProperty(property);
            // Add an additional line and check if property.isExpanded
            int lines = sceneAssetProperty.objectReferenceValue != null ? 3 : 2;
            // If this oneliner is confusing you - it does the same as
            //var line = 3; // Fully expanded and with info
            //if(sceneAssetProperty.objectReferenceValue == null) line = 2;
            //if(!property.isExpanded) line = 1;

            return BoxPadding.vertical + LineHeight * lines + PadSize * (lines - 1) + FooterHeight;
        }

        /// <summary>
        /// Draws info box of the provided scene
        /// </summary>
        private void DrawSceneInfoGUI(Rect position, BuildUtils.BuildScene buildScene, int sceneControlID) {
            bool readOnly = BuildUtils.IsReadOnly();
            string readOnlyWarning = readOnly ? "\n\nWARNING: Build Settings is not checked out and so cannot be modified." : "";

            // Label Prefix
            GUIContent iconContent = new GUIContent();
            GUIContent labelContent = new GUIContent();
            GUIContent labellevelContent = new GUIContent();
            GUIContent iconlevelContent = new GUIContent();

            // Missing from build scenes
            if (buildScene.BuildIndex == -1) {
                iconContent = EditorGUIUtility.IconContent("d_winbtn_mac_close");
                labelContent.text = "NOT In Build";
                labelContent.tooltip = "This scene is NOT in build settings.\nIt will be NOT included in builds.";
            }
            // In build scenes and enabled
            else if (buildScene.Scene.enabled) {
                iconContent = EditorGUIUtility.IconContent("d_winbtn_mac_max");
                labelContent.text = $"BuildIndex: {buildScene.BuildIndex}";
                labelContent.tooltip = $"This scene is in build settings and ENABLED.\nIt will be included in builds.{readOnlyWarning}";
            }
            // In build scenes and disabled
            else {
                iconContent = EditorGUIUtility.IconContent("d_winbtn_mac_min");
                labelContent.text = $"BuildIndex: {buildScene.BuildIndex}";
                labelContent.tooltip = "This scene is in build settings and DISABLED.\nIt will be NOT included in builds.";
            }

            //d_Profiler.GlobalIllumination
            //d_PlatformEffector2D Icon
            //d_TerrainCollider Icon
            //TerrainCollider Icon
            bool isLevel = MtwScenes.SceneIsALevel(buildScene.AssetPath);
            iconlevelContent = EditorGUIUtility.IconContent(isLevel ? "TerrainCollider Icon" : "d_Profiler.Memory");
            iconlevelContent.tooltip = isLevel ? "The scene is an Level" : "The scene is an Technical";
            iconlevelContent.text = isLevel ? "Level" : "Technical";


            // Left status label
            using (new EditorGUI.DisabledScope(readOnly)) {
                Rect labelRect = DrawUtils.GetLabelRect(position);
                Rect iconRect = labelRect;
                Rect level = labelRect;
                iconRect.width = iconContent.image.width + PadSize;
                labelRect.width -= iconRect.width;
                labelRect.x += iconRect.width;
                level = labelRect;
                level.x += 100;
                level.width = 80;
                EditorGUI.PrefixLabel(iconRect, sceneControlID, iconContent);
                EditorGUI.PrefixLabel(labelRect, sceneControlID, labelContent);
                EditorGUI.PrefixLabel(level, sceneControlID, iconlevelContent);
                EditorGUI.PrefixLabel(level, sceneControlID, labellevelContent);
            }

            // Right context buttons
            Rect buttonRect = DrawUtils.GetFieldRect(position);
            buttonRect.width = (buttonRect.width) / 3;

            string tooltipMsg = "";
            using (new EditorGUI.DisabledScope(readOnly)) {
                // NOT in build settings
                if (buildScene.BuildIndex == -1) {
                    buttonRect.width *= 2;
                    int addIndex = EditorBuildSettings.scenes.Length;
                    tooltipMsg = $"Add this scene to build settings. It will be appended to the end of the build scenes as buildIndex: {addIndex}.{readOnlyWarning}";
                    if (DrawUtils.ButtonHelper(buttonRect, "Add...", $"Add (buildIndex {addIndex})", EditorStyles.miniButtonLeft, tooltipMsg))
                        BuildUtils.AddBuildScene(buildScene);
                    buttonRect.width /= 2;
                    buttonRect.x += buttonRect.width;
                }
                // In build settings
                else {
                    bool isEnabled = buildScene.Scene.enabled;
                    string stateString = isEnabled ? "Disable" : "Enable";
                    tooltipMsg = $"{stateString} this scene in build settings.\n{(isEnabled ? "It will no longer be included in builds" : "It will be included in builds")}.{readOnlyWarning}";

                    if (DrawUtils.ButtonHelper(buttonRect, stateString, $"{stateString} In Build", EditorStyles.miniButtonLeft, tooltipMsg))
                        BuildUtils.SetBuildSceneState(buildScene, !isEnabled);
                    buttonRect.x += buttonRect.width;

                    tooltipMsg = $"Completely remove this scene from build settings.\nYou will need to add it again for it to be included in builds!{readOnlyWarning}";
                    if (DrawUtils.ButtonHelper(buttonRect, "Remove...", "Remove from Build", EditorStyles.miniButtonMid, tooltipMsg))
                        BuildUtils.RemoveBuildScene(buildScene);
                }
            }

            buttonRect.x += buttonRect.width;

            tooltipMsg = $"Open the 'Build Settings' Window for managing scenes.{readOnlyWarning}";
            if (DrawUtils.ButtonHelper(buttonRect, "Settings", "Build Settings", EditorStyles.miniButtonRight, tooltipMsg)) {
                BuildUtils.OpenBuildSettings();
            }
        }

        private static SerializedProperty GetSceneAssetProperty(SerializedProperty property) {
            return property.FindPropertyRelative(SceneAssetPropertyString);
        }

        private static SerializedProperty GetScenePathProperty(SerializedProperty property) {
            return property.FindPropertyRelative(ScenePathPropertyString);
        }

        private static class DrawUtils {
            /// <summary>
            /// Draw a GUI button, choosing between a short and a long button text based on if it fits
            /// </summary>
            public static bool ButtonHelper(Rect position, string msgShort, string msgLong, GUIStyle style, string tooltip = null) {
                GUIContent content = new GUIContent(msgLong) { tooltip = tooltip };

                float longWidth = style.CalcSize(content).x;
                if (longWidth > position.width) content.text = msgShort;

                return GUI.Button(position, content, style);
            }

            /// <summary>
            /// Given a position rect, get its field portion
            /// </summary>
            public static Rect GetFieldRect(Rect position) {
                position.width -= EditorGUIUtility.labelWidth;
                position.x += EditorGUIUtility.labelWidth;
                return position;
            }

            /// <summary>
            /// Given a position rect, get its label portion
            /// </summary>
            public static Rect GetLabelRect(Rect position) {
                position.width = EditorGUIUtility.labelWidth - PadSize;
                return position;
            }
        }

        /// <summary>
        /// Various BuildSettings interactions
        /// </summary>
        private static class BuildUtils {
            // time in seconds that we have to wait before we query again when IsReadOnly() is called.
            public static float MINCheckWait = 3;

            private static float _lastTimeChecked;
            private static bool _cachedReadonlyVal = true;

            /// <summary>
            /// A small container for tracking scene data BuildSettings
            /// </summary>
            public struct BuildScene {
                public int BuildIndex;
                public GUID AssetGuid;
                public string AssetPath;
                public EditorBuildSettingsScene Scene;
            }

            /// <summary>
            /// Check if the build settings asset is readonly.
            /// Caches value and only queries state a max of every 'minCheckWait' seconds.
            /// </summary>
            public static bool IsReadOnly() {
                float curTime = Time.realtimeSinceStartup;
                float timeSinceLastCheck = curTime - _lastTimeChecked;

                if (!(timeSinceLastCheck > MINCheckWait)) return _cachedReadonlyVal;

                _lastTimeChecked = curTime;
                _cachedReadonlyVal = QueryBuildSettingsStatus();

                return _cachedReadonlyVal;
            }

            /// <summary>
            /// A blocking call to the Version Control system to see if the build settings asset is readonly.
            /// Use BuildSettingsIsReadOnly for version that caches the value for better responsivenes.
            /// </summary>
            private static bool QueryBuildSettingsStatus() {
                // If no version control provider, assume not readonly
                if (!Provider.enabled) return false;

                // If we cannot checkout, then assume we are not readonly
                if (!Provider.hasCheckoutSupport) return false;

                //// If offline (and are using a version control provider that requires checkout) we cannot edit.
                //if (UnityEditor.VersionControl.Provider.onlineState == UnityEditor.VersionControl.OnlineState.Offline)
                //    return true;

                // Try to get status for file
                Task status = Provider.Status("ProjectSettings/EditorBuildSettings.asset", false);
                status.Wait();

                // If no status listed we can edit
                if (status.assetList == null || status.assetList.Count != 1) return true;

                // If is checked out, we can edit
                return !status.assetList[0].IsState(Asset.States.CheckedOutLocal);
            }

            /// <summary>
            /// For a given Scene Asset object reference, extract its build settings data, including buildIndex.
            /// </summary>
            public static BuildScene GetBuildScene(Object sceneObject) {
                BuildScene entry = new BuildScene {
                    BuildIndex = -1,
                    AssetGuid = new GUID(string.Empty)
                };

                if (sceneObject as SceneAsset == null) return entry;

                entry.AssetPath = AssetDatabase.GetAssetPath(sceneObject);
                entry.AssetGuid = new GUID(AssetDatabase.AssetPathToGUID(entry.AssetPath));

                EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
                for (int index = 0; index < scenes.Length; ++index) {
                    if (!entry.AssetGuid.Equals(scenes[index].guid)) continue;

                    entry.Scene = scenes[index];
                    entry.BuildIndex = index;
                    return entry;
                }

                return entry;
            }

            /// <summary>
            /// Enable/Disable a given scene in the buildSettings
            /// </summary>
            public static void SetBuildSceneState(BuildScene buildScene, bool enabled) {
                bool modified = false;
                EditorBuildSettingsScene[] scenesToModify = EditorBuildSettings.scenes;
                foreach (EditorBuildSettingsScene curScene in scenesToModify.Where(curScene => curScene.guid.Equals(buildScene.AssetGuid))) {
                    curScene.enabled = enabled;
                    modified = true;
                    break;
                }

                if (modified) EditorBuildSettings.scenes = scenesToModify;
            }

            /// <summary>
            /// Display Dialog to add a scene to build settings
            /// </summary>
            public static void AddBuildScene(BuildScene buildScene, bool force = false, bool enabled = true) {
                if (force == false) {
                    int selection = EditorUtility.DisplayDialogComplex(
                        "Add Scene To Build",
                        $"You are about to add scene at {buildScene.AssetPath} To the Build Settings.",
                        "Add as Enabled", // option 0
                        "Add as Disabled", // option 1
                        "Cancel (do nothing)"); // option 2

                    switch (selection) {
                        case 0: // enabled
                            enabled = true;
                            break;
                        case 1: // disabled
                            enabled = false;
                            break;
                        default:
                            //case 2: // cancel
                            return;
                    }
                }

                EditorBuildSettingsScene newScene = new EditorBuildSettingsScene(buildScene.AssetGuid, enabled);
                List<EditorBuildSettingsScene> tempScenes = EditorBuildSettings.scenes.ToList();
                tempScenes.Add(newScene);
                EditorBuildSettings.scenes = tempScenes.ToArray();
            }

            /// <summary>
            /// Display Dialog to remove a scene from build settings (or just disable it)
            /// </summary>
            public static void RemoveBuildScene(BuildScene buildScene, bool force = false) {
                bool onlyDisable = false;
                if (force == false) {
                    int selection = -1;

                    string title = "Remove Scene From Build";
                    string details =
                        $"You are about to remove the following scene from build settings:\n    {buildScene.AssetPath}\n    buildIndex: {buildScene.BuildIndex}\n\nThis will modify build settings, but the scene asset will remain untouched.";
                    string confirm = "Remove From Build";
                    string alt = "Just Disable";
                    string cancel = "Cancel (do nothing)";

                    if (buildScene.Scene.enabled) {
                        details += "\n\nIf you want, you can also just disable it instead.";
                        selection = EditorUtility.DisplayDialogComplex(title, details, confirm, alt, cancel);
                    }
                    else {
                        selection = EditorUtility.DisplayDialog(title, details, confirm, cancel) ? 0 : 2;
                    }

                    switch (selection) {
                        case 0: // remove
                            break;
                        case 1: // disable
                            onlyDisable = true;
                            break;
                        default:
                            //case 2: // cancel
                            return;
                    }
                }

                // User chose to not remove, only disable the scene
                if (onlyDisable) {
                    SetBuildSceneState(buildScene, false);
                }
                // User chose to fully remove the scene from build settings
                else {
                    List<EditorBuildSettingsScene> tempScenes = EditorBuildSettings.scenes.ToList();
                    tempScenes.RemoveAll(scene => scene.guid.Equals(buildScene.AssetGuid));
                    EditorBuildSettings.scenes = tempScenes.ToArray();
                }
            }

            /// <summary>
            /// Open the default Unity Build Settings window
            /// </summary>
            public static void OpenBuildSettings() {
                EditorWindow.GetWindow(typeof(BuildPlayerWindow));
            }
        }
    }
}
#endif