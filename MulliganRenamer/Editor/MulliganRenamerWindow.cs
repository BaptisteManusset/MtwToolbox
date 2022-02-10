/* MIT License

Copyright (c) 2016 Edward Rowe, RedBlueGames

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

namespace RedBlueGames.MulliganRenamer {
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Tool that tries to allow renaming mulitple selections by parsing similar substrings
    /// </summary>
    public class MulliganRenamerWindow : EditorWindow {
        private const string VersionString = "1.6.0";
        private const string WindowMenuPath = "Tools/Mulligan Renamer";

        private const string RenameOpsEditorPrefsKey = "RedBlueGames.MulliganRenamer.RenameOperationsToApply";
        private const string UserPreferencesPrefKey = "RedBlueGames.MulliganRenamer.UserPreferences";
        private const string PreviewModePrefixKey = "RedBlueGames.MulliganRenamer.IsPreviewStepModePreference";

        private const float OperationPanelWidth = 350.0f;

        private GUIStyles guiStyles;
        private GUIContents guiContents;

        private Vector2 renameOperationsPanelScrollPosition;
        private Vector2 previewPanelScrollPosition;
        private MulliganRenamerPreviewPanel previewPanel;
        private SavePresetWindow activeSavePresetWindow;
        private ManagePresetsWindow activePresetManagementWindow;

        private int NumPreviouslyRenamedObjects { get; set; }

        private BulkRenamer BulkRenamer { get; set; }

        private BulkRenamePreview BulkRenamePreview { get; set; }

        private List<RenameOperationDrawerBinding> RenameOperationDrawerBindingPrototypes { get; set; }

        private UniqueList<Object> ObjectsToRename { get; set; }

        private List<RenameOperationDrawerBinding> RenameOperationsToApplyWithBindings { get; set; }

        private MulliganUserPreferences ActivePreferences { get; set; }

        private string CurrentPresetName { get; set; }

        private bool IsNewSession { get; set; }

        private bool IsShowingThanksForReview { get; set; }

        private int NumRenameOperations {
            get { return RenameOperationsToApplyWithBindings.Count; }
        }

        private IRenameOperation OperationToForceFocus { get; set; }

        private int FocusedRenameOpIndex {
            get {
                var focusedControl = GUI.GetNameOfFocusedControl();
                if (string.IsNullOrEmpty(focusedControl)) {
                    return -1;
                }

                if (!GUIControlNameUtility.IsControlNamePrefixed(focusedControl)) {
                    return -1;
                }

                return GUIControlNameUtility.GetPrefixFromName(focusedControl);
            }
        }

        private IRenameOperation FocusedRenameOp {
            get {
                var focusedOpIndex = FocusedRenameOpIndex;
                if (focusedOpIndex >= 0 && focusedOpIndex < NumRenameOperations) {
                    return RenameOperationsToApplyWithBindings[FocusedRenameOpIndex].Operation;
                }
                else {
                    return null;
                }
            }
        }

        private bool IsShowingPreviewSteps {
            get {
                // Show step previewing mode when only one operation is left because Results mode is pointless with one op only.
                // But don't actually change the mode preference so that adding ops restores whatever mode the user was in.
                return IsPreviewStepModePreference || NumRenameOperations <= 1;
            }
        }

        private string LastFocusedControlName { get; set; }

        private bool IsPreviewStepModePreference {
            get { return EditorPrefs.GetBool(PreviewModePrefixKey, true); }

            set { EditorPrefs.SetBool(PreviewModePrefixKey, value); }
        }

        private List<Object> ValidSelectedObjects { get; set; }

        private bool NeedsReview {
            get { return ActivePreferences.NeedsReview || IsShowingThanksForReview; }
        }

        [MenuItem(WindowMenuPath, false)]
        private static void ShowWindow() {
            var bulkRenamerWindow = GetWindow<MulliganRenamerWindow>(false, "Mulligan Renamer", true);

            // When they launch via right click, we immediately load the objects in.
            bulkRenamerWindow.LoadSelectedObjects();
        }

        private static bool ObjectIsRenamable(Object obj) {
            // Workaround for Issue #200 where AssetDatabase call during EditorApplicationUpdate caused a Null Reference Exception
            bool objectIsAsset = false;
            try {
                objectIsAsset = AssetDatabase.Contains(obj);
            }
            catch (System.NullReferenceException) {
                // Can't access the AssetDatabase at this time.
                return false;
            }

            if (objectIsAsset) {
                // Only sub assets of sprites are currently supported, so let's just not let them be added.
                if (AssetDatabase.IsSubAsset(obj) && !(obj is Sprite)) {
                    return false;
                }

                // Create -> Prefab results in assets that have no name. Typically you can't have Assets that have no name,
                // so we will just ignore them for the utility.
                return !string.IsNullOrEmpty(obj.name);
            }

            if (obj is GameObject) {
                return true;
            }

            return false;
        }

        private static bool DrawPreviewBreadcrumb(Rect rect, PreviewBreadcrumbOptions breacrumbConfig) {
            var styleName = breacrumbConfig.StyleName;
            var enabled = breacrumbConfig.Enabled;
            bool selected = GUI.Toggle(rect, enabled, breacrumbConfig.Heading, styleName);
            if (selected) {
                var coloredHighlightRect = new Rect(rect);
                coloredHighlightRect.height = 2;
                coloredHighlightRect.width += 1.0f;
                coloredHighlightRect.x += breacrumbConfig.UseLeftStyle ? -5.0f : -4.0f;
                var oldColor = GUI.color;
                GUI.color = breacrumbConfig.HighlightColor;
                GUI.DrawTexture(coloredHighlightRect, Texture2D.whiteTexture);
                GUI.color = oldColor;
            }

            return selected;
        }

        private void OnEnable() {
            AssetPreview.SetPreviewTextureCacheSize(100);
            minSize = new Vector2(600.0f, 300.0f);

            previewPanelScrollPosition = Vector2.zero;

            RenameOperationsToApplyWithBindings = new List<RenameOperationDrawerBinding>();
            ObjectsToRename = new UniqueList<Object>();

            CacheRenameOperationPrototypes();

            CurrentPresetName = string.Empty;
            LoadUserPreferences();

            // Intentionally forget their last preset when opening the window, because the user won't
            // remember they previously loaded a preset. It will only confuse them if the Save As
            // is populated with this name.
            CurrentPresetName = string.Empty;

            IsNewSession = true;
            IsShowingThanksForReview = false;

            BulkRenamer = new BulkRenamer();
            Selection.selectionChanged += Repaint;

            EditorApplication.update += CacheBulkRenamerPreview;
            EditorApplication.update += CacheValidSelectedObjects;

            // Sometimes, GUI happens before Editor Update, so also cache a preview now.
            CacheBulkRenamerPreview();
            CacheValidSelectedObjects();
        }

        private void CacheBulkRenamerPreview() {
            var operationSequence = GetCurrentRenameOperationSequence();
            BulkRenamer.SetRenameOperations(operationSequence);
            BulkRenamePreview = BulkRenamer.GetBulkRenamePreview(ObjectsToRename.ToList());
        }

        private void CacheValidSelectedObjects() {
            ValidSelectedObjects = Selection.objects.Where((obj) => ObjectIsValidForRename(obj)).ToList();
        }

        private void InitializePreviewPanel() {
            previewPanel = new MulliganRenamerPreviewPanel();
            previewPanel.ValidateObject = ObjectIsValidForRename;
            previewPanel.ObjectsDropped += HandleObjectsDroppedOverPreviewArea;
            previewPanel.RemoveAllClicked += HandleRemoveAllObjectsClicked;
            previewPanel.AddSelectedObjectsClicked += HandleAddSelectedObjectsClicked;
            previewPanel.ObjectRemovedAtIndex += HandleObjectRemoved;
        }

        private void HandleObjectsDroppedOverPreviewArea(Object[] objects) {
            AddObjectsToRename(objects);
            ScrollPreviewPanelToBottom();
        }

        private void HandleRemoveAllObjectsClicked() {
            ObjectsToRename.Clear();
        }

        private void HandleAddSelectedObjectsClicked() {
            LoadSelectedObjects();
        }

        private void HandleObjectRemoved(int index) {
            ObjectsToRename.RemoveAt(index);
        }

        private void OnDisable() {
            SaveUserPreferences();

            // If they've opened up the save preset window and are closing mulligan window, close the save preset
            // window because it can cause bugs since it can still invoke callbacks.
            // Same for presets window.
            if (activeSavePresetWindow != null) {
                activeSavePresetWindow.Close();
            }

            if (activePresetManagementWindow != null) {
                activePresetManagementWindow.Close();
            }

            Selection.selectionChanged -= Repaint;
            EditorApplication.update -= CacheBulkRenamerPreview;
            EditorApplication.update -= CacheValidSelectedObjects;
        }

        private void CacheRenameOperationPrototypes() {
            // This binds Operations to their respective Drawers
            RenameOperationDrawerBindingPrototypes = new List<RenameOperationDrawerBinding>();
            RenameOperationDrawerBindingPrototypes.Add(
                new RenameOperationDrawerBinding(new ReplaceStringOperation(), new ReplaceStringOperationDrawer()));

            RenameOperationDrawerBindingPrototypes.Add(
                new RenameOperationDrawerBinding(new ReplaceNameOperation(), new ReplaceNameOperationDrawer()));

            RenameOperationDrawerBindingPrototypes.Add(
                new RenameOperationDrawerBinding(new AddStringOperation(), new AddStringOperationDrawer()));

            RenameOperationDrawerBindingPrototypes.Add(
                new RenameOperationDrawerBinding(new EnumerateOperation(), new EnumerateOperationDrawer()));

            RenameOperationDrawerBindingPrototypes.Add(
                new RenameOperationDrawerBinding(new CountByLetterOperation(), new CountByLetterOperationDrawer()));

            RenameOperationDrawerBindingPrototypes.Add(
                new RenameOperationDrawerBinding(new AddStringSequenceOperation(), new AddStringSequenceOperationDrawer()));

            RenameOperationDrawerBindingPrototypes.Add(
                new RenameOperationDrawerBinding(new ChangeCaseOperation(), new ChangeCaseOperationDrawer()));

            RenameOperationDrawerBindingPrototypes.Add(
                new RenameOperationDrawerBinding(new ToCamelCaseOperation(), new ToCamelCaseOperationDrawer()));

            RenameOperationDrawerBindingPrototypes.Add(
                new RenameOperationDrawerBinding(new TrimCharactersOperation(), new TrimCharactersOperationDrawer()));

            RenameOperationDrawerBindingPrototypes.Add(
                new RenameOperationDrawerBinding(new RemoveCharactersOperation(), new RemoveCharactersOperationDrawer()));
        }

        private void InitializeGUIContents() {
            guiContents = new GUIContents();

            var copyrightLabel = $"Mulligan Renamer v{VersionString}, ©2018 RedBlueGames";
            guiContents.CopyrightLabel = new GUIContent(copyrightLabel);
        }

        private void InitializeGUIStyles() {
            guiStyles = new GUIStyles();

            var copyrightStyle = new GUIStyle(EditorStyles.miniLabel);
            copyrightStyle.alignment = TextAnchor.MiddleRight;
            guiStyles.CopyrightLabel = copyrightStyle;
        }

        private void OnGUI() {
            // Initialize GUIContents and GUIStyles in OnGUI since it makes calls that must be done in OnGUI loop.
            if (guiContents == null) {
                InitializeGUIContents();
            }

            if (guiStyles == null) {
                InitializeGUIStyles();
            }

            // Remove any objects that got deleted while working
            ObjectsToRename.RemoveNullObjects();

            // Breadcrumbs take up more than a single line so we add a bit more
            var toolbarRect = new Rect(0.0f, 0.0f, position.width, EditorGUIUtility.singleLineHeight + 3.0f);
            DrawToolbar(toolbarRect);

            var reviewPromptHeight = 0.0f;
            if (NeedsReview) {
                // Responsiveness: Expand height as the window shrinks to better fit the text
                if (position.width > 800.0f) {
                    reviewPromptHeight = 38.0f;
                }
                else {
                    reviewPromptHeight = 48.0f;
                }
            }

            var reviewPromptPaddingY = 16.0f;
            var footerHeight = 60.0f + reviewPromptHeight + reviewPromptPaddingY;
            var operationPanelRect = new Rect(
                0.0f,
                0.0f,
                OperationPanelWidth,
                position.height - toolbarRect.height - footerHeight);
            DrawOperationsPanel(operationPanelRect);

            FocusForcedFocusControl();

            var previewPanelPadding = new RectOffset(1, 1, -1, 0);
            var previewPanelRect = new Rect(
                operationPanelRect.width + previewPanelPadding.left,
                toolbarRect.height + previewPanelPadding.top,
                position.width - operationPanelRect.width - previewPanelPadding.left - previewPanelPadding.right,
                position.height - toolbarRect.height - footerHeight - previewPanelPadding.top - previewPanelPadding.bottom);

            DrawPreviewPanel(previewPanelRect, BulkRenamePreview);

            var rectForReviewWidth = position.width * 0.98f;
            var rectForReviewPrompt = new Rect(
                (position.width - rectForReviewWidth) * 0.5f,
                previewPanelRect.y + previewPanelRect.height + reviewPromptPaddingY,
                rectForReviewWidth,
                reviewPromptHeight);

            if (NeedsReview) {
                DrawReviewPrompt(rectForReviewPrompt);
            }

            var disableRenameButton =
                RenameOperatationsHaveErrors() ||
                ObjectsToRename.Count == 0;
            EditorGUI.BeginDisabledGroup(disableRenameButton);
            var renameButtonPadding = new Vector4(30.0f, 16.0f, 30.0f, 16.0f);
            var renameButtonSize = new Vector2(position.width - renameButtonPadding.x - renameButtonPadding.z, 24.0f);
            var renameButtonRect = new Rect(
                renameButtonPadding.x,
                rectForReviewPrompt.y + rectForReviewPrompt.height + renameButtonPadding.y,
                renameButtonSize.x,
                renameButtonSize.y);

            if (GUI.Button(renameButtonRect, "Rename")) {
                var popupMessage = string.Concat(
                    "Some objects have warnings and will not be renamed. Do you want to rename the other objects in the group?");
                var renamesHaveNoWarnings = !BulkRenamePreview.HasWarnings;
                if (renamesHaveNoWarnings || EditorUtility.DisplayDialog("Warning", popupMessage, "Rename", "Cancel")) {
                    var undoGroupBeforeRename = Undo.GetCurrentGroup();
                    try {
                        NumPreviouslyRenamedObjects = BulkRenamer.RenameObjects(ObjectsToRename.ToList());
                        ObjectsToRename.Clear();
                        if (IsNewSession) {
                            ActivePreferences.NumSessionsUsed++;
                            IsNewSession = false;
                        }
                    }
                    catch (System.OperationCanceledException e) {
                        var errorMessage =
                            $"{"Sorry, some objects failed to rename. Something went wrong with Mulligan." + "Please report a bug (see UserManual for details). This rename operation will be automatically undone"}\n\nException: {e.Message}";
                        if (EditorUtility.DisplayDialog("Error", errorMessage, "Ok")) {
                            Undo.RevertAllDownToGroup(undoGroupBeforeRename);
                        }

                        GUIUtility.ExitGUI();
                    }
                }

                // Opening the dialog breaks the layout stack, so ExitGUI to prevent a NullPtr.
                // https://answers.unity.com/questions/1353442/editorutilitysavefilepane-and-beginhorizontal-caus.html
                // NOTE: This may no longer be necessary after reworking the gui to use non-layout.
                GUIUtility.ExitGUI();
            }

            EditorGUI.EndDisabledGroup();

            var copyrightRect = new Rect(
                0.0f,
                renameButtonRect.y + renameButtonRect.height + 2.0f,
                position.width,
                EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(copyrightRect, guiContents.CopyrightLabel, guiStyles.CopyrightLabel);

            // Issue #115 - Workaround to force focus to stay with whatever widget it was previously on...
            var focusedControl = GUI.GetNameOfFocusedControl();
            if (string.IsNullOrEmpty(focusedControl)) {
                GUI.FocusControl(LastFocusedControlName);
                EditorGUI.FocusTextInControl(LastFocusedControlName);
            }
            else {
                LastFocusedControlName = GUI.GetNameOfFocusedControl();
            }
        }

        private void DrawToolbar(Rect toolbarRect) {
            var operationStyle = new GUIStyle(EditorStyles.toolbar);
            GUI.Box(toolbarRect, "", operationStyle);

            // The breadcrumb style spills to the left some so we need to claim extra space for it
            const float BreadcrumbLeftOffset = 7.0f;
            var breadcrumbRect = new Rect(
                new Vector2(BreadcrumbLeftOffset + OperationPanelWidth, toolbarRect.y),
                new Vector2(toolbarRect.width - OperationPanelWidth - BreadcrumbLeftOffset, toolbarRect.height));

            DrawBreadcrumbs(IsShowingPreviewSteps, breadcrumbRect);

            EditorGUI.BeginDisabledGroup(NumRenameOperations <= 1);
            var buttonText = "Preview Steps";
            var previewButtonSize = new Vector2(100.0f, toolbarRect.height);
            var previewButtonPosition = new Vector2(toolbarRect.xMax - previewButtonSize.x, toolbarRect.y);
            var toggleRect = new Rect(previewButtonPosition, previewButtonSize);
            IsPreviewStepModePreference = GUI.Toggle(toggleRect, IsPreviewStepModePreference, buttonText, "toolbarbutton");
            EditorGUI.EndDisabledGroup();
        }

        private void DrawBreadcrumbs(bool isShowingPreviewSteps, Rect rect) {
            if (NumRenameOperations == 0) {
                var emptyBreadcrumbRect = new Rect(rect);
                emptyBreadcrumbRect.width = 20.0f;
                EditorGUI.BeginDisabledGroup(true);
                GUI.Toggle(emptyBreadcrumbRect, false, string.Empty, "GUIEditor.BreadcrumbLeft");
                EditorGUI.EndDisabledGroup();
            }
            else {
                if (isShowingPreviewSteps) {
                    var totalWidth = 0.0f;
                    for (int i = 0; i < NumRenameOperations; ++i) {
                        var drawerAtBreadcrumb = RenameOperationsToApplyWithBindings[i].Drawer;
                        var breadcrumbOption = new PreviewBreadcrumbOptions();
                        breadcrumbOption.Heading = drawerAtBreadcrumb.HeadingLabel;
                        breadcrumbOption.HighlightColor = drawerAtBreadcrumb.HighlightColor;

                        breadcrumbOption.UseLeftStyle = i == 0;
                        breadcrumbOption.Enabled = i == FocusedRenameOpIndex;

                        var breadcrumbPosition = new Vector2(rect.x + totalWidth, rect.y);

                        var nextBreadcrumbRect = new Rect(breadcrumbPosition, breadcrumbOption.SizeForContent);
                        nextBreadcrumbRect.position = new Vector2(rect.x + totalWidth, rect.y);

                        var selected = DrawPreviewBreadcrumb(nextBreadcrumbRect, breadcrumbOption);
                        if (selected && i != FocusedRenameOpIndex) {
                            var renameOp = RenameOperationsToApplyWithBindings[i].Operation;
                            FocusRenameOperationDeferred(renameOp);
                        }

                        totalWidth += nextBreadcrumbRect.width;
                    }
                }
                else {
                    var breadcrumbeOptions =
                        new PreviewBreadcrumbOptions() {Heading = "Result", HighlightColor = Color.clear, Enabled = true, UseLeftStyle = true};
                    var breadcrumbSize = breadcrumbeOptions.SizeForContent;
                    breadcrumbSize.y = rect.height;
                    DrawPreviewBreadcrumb(new Rect(rect.position, breadcrumbSize), breadcrumbeOptions);
                }
            }
        }

        private void DrawOperationsPanel(Rect operationPanelRect) {
            var totalHeightOfOperations = 0.0f;
            for (int i = 0; i < NumRenameOperations; ++i) {
                var drawer = RenameOperationsToApplyWithBindings[i].Drawer;
                totalHeightOfOperations += drawer.GetPreferredHeight();
            }

            var headerRect = new Rect(operationPanelRect);
            headerRect.height = 18.0f;
            DrawOperationsPanelHeader(headerRect);

            var scrollAreaRect = new Rect(operationPanelRect);
            scrollAreaRect.y += headerRect.height;
            scrollAreaRect.height -= headerRect.height;

            var buttonSize = new Vector2(150.0f, 20.0f);
            var spaceBetweenButton = 16.0f;
            var scrollContentsRect = new Rect(scrollAreaRect);
            scrollContentsRect.height = totalHeightOfOperations + spaceBetweenButton + buttonSize.y;

            // If we need to scroll vertically, subtract out room for the vertical scrollbar so we don't
            // have to also scroll horiztonally
            var contentsFit = scrollContentsRect.height <= scrollAreaRect.height;
            if (!contentsFit) {
                scrollContentsRect.width -= 15.0f;
            }

            renameOperationsPanelScrollPosition = GUI.BeginScrollView(
                scrollAreaRect,
                renameOperationsPanelScrollPosition,
                scrollContentsRect);

            DrawRenameOperations(scrollContentsRect);

            var buttonRect = new Rect();
            buttonRect.x = scrollContentsRect.x + (scrollContentsRect.size.x / 2.0f) - (buttonSize.x / 2.0f);
            buttonRect.y = scrollContentsRect.y + scrollContentsRect.height - buttonSize.y;
            buttonRect.height = buttonSize.y;
            buttonRect.width = buttonSize.x;

            if (GUI.Button(buttonRect, "Add Operation")) {
                // Add enums to the menu
                var menu = new GenericMenu();
                foreach (var renameOpDrawerBindingPrototype in RenameOperationDrawerBindingPrototypes) {
                    var content = new GUIContent(renameOpDrawerBindingPrototype.Drawer.MenuDisplayPath);
                    menu.AddItem(content, false, OnAddRenameOperationConfirmed, renameOpDrawerBindingPrototype);
                }

                menu.ShowAsContext();
            }

            GUI.EndScrollView();
        }

        private void DrawOperationsPanelHeader(Rect headerRect) {
            var headerStyle = new GUIStyle(EditorStyles.toolbar);
            GUI.Box(headerRect, "", headerStyle);
            var headerLabelStyle = new GUIStyle(EditorStyles.boldLabel);
            headerLabelStyle.alignment = TextAnchor.MiddleLeft;
            var headerLabelRect = new Rect(headerRect);
            headerLabelRect.x += 2.0f;
            headerLabelRect.width -= 2.0f;

            var headerLabel = "Rename Operations";
            var renameOpsLabel = new GUIContent(headerLabel);
            EditorGUI.LabelField(headerLabelRect, renameOpsLabel, headerLabelStyle);

            var presetButtonsRect = new Rect(headerRect);
            presetButtonsRect.width = 60.0f;
            presetButtonsRect.x = headerRect.width - presetButtonsRect.width;
            var useDebugPresets = Event.current.shift;
            if (GUI.Button(presetButtonsRect, "Presets", EditorStyles.toolbarDropDown)) {
                var menu = new GenericMenu();
                var savedPresetNames = new string[ActivePreferences.SavedPresets.Count];
                for (int i = 0; i < ActivePreferences.SavedPresets.Count; ++i) {
                    savedPresetNames[i] = ActivePreferences.SavedPresets[i].Name;
                }

                for (int i = 0; i < savedPresetNames.Length; ++i) {
                    var content = new GUIContent(savedPresetNames[i]);
                    int copyI = i;
                    menu.AddItem(content, false, () => {
                        var preset = ActivePreferences.SavedPresets[copyI];
                        LoadPreset(preset);
                    });
                }

                menu.AddSeparator(string.Empty);
                menu.AddItem(new GUIContent("Save As..."), false, () => ShowSavePresetWindow());
                menu.AddItem(new GUIContent("Manage Presets..."), false, () => ShowManagePresetsWindow());
                if (useDebugPresets) {
                    menu.AddItem(new GUIContent("DEBUG - Delete UserPrefs"), false, () => {
                        ActivePreferences = new MulliganUserPreferences();
                        SaveUserPreferences();
                    });
                }

                menu.ShowAsContext();
            }
        }

        private void DrawRenameOperations(Rect operationsRect) {
            // Store the op before buttons are pressed because buttons change focus
            var focusedOpBeforeButtonPresses = FocusedRenameOp;
            bool saveOpsToPreferences = false;
            IRenameOperation operationToFocus = null;

            var totalHeightDrawn = 0.0f;
            for (int i = 0; i < NumRenameOperations; ++i) {
                var currentElement = RenameOperationsToApplyWithBindings[i];
                var rect = new Rect(operationsRect);
                rect.y += totalHeightDrawn;
                rect.height = currentElement.Drawer.GetPreferredHeight();
                totalHeightDrawn += rect.height;

                var guiOptions = new RenameOperationGUIOptions();
                guiOptions.ControlPrefix = i;
                guiOptions.DisableUpButton = i == 0;
                guiOptions.DisableDownButton = i == NumRenameOperations - 1;
                var buttonClickEvent = currentElement.Drawer.DrawGUI(rect, guiOptions);
                switch (buttonClickEvent) {
                    case RenameOperationSortingButtonEvent.MoveUp: {
                        RenameOperationsToApplyWithBindings.MoveElementFromIndexToIndex(i, i - 1);
                        saveOpsToPreferences = true;

                        // Move focus with the RenameOp. This techincally changes their focus within the 
                        // rename op, but it's better than focus getting swapped to whatever op replaces this one.
                        operationToFocus = focusedOpBeforeButtonPresses;
                        break;
                    }

                    case RenameOperationSortingButtonEvent.MoveDown: {
                        RenameOperationsToApplyWithBindings.MoveElementFromIndexToIndex(i, i + 1);
                        saveOpsToPreferences = true;
                        operationToFocus = focusedOpBeforeButtonPresses;
                        break;
                    }

                    case RenameOperationSortingButtonEvent.Delete: {
                        var removingFocusedOperation = focusedOpBeforeButtonPresses == currentElement;

                        RenameOperationsToApplyWithBindings.RemoveAt(i);
                        saveOpsToPreferences = true;

                        if (removingFocusedOperation && NumRenameOperations > 0) {
                            // Focus the RenameOp that took this one's place, if there is one. 
                            var indexToFocus = Mathf.Min(NumRenameOperations - 1, i);
                            operationToFocus = RenameOperationsToApplyWithBindings[indexToFocus].Operation;
                        }
                        else {
                            operationToFocus = focusedOpBeforeButtonPresses;
                        }

                        break;
                    }

                    case RenameOperationSortingButtonEvent.None: {
                        // Do nothing
                        break;
                    }

                    default: {
                        Debug.LogError(string.Format(
                            "RenamerWindow found Unrecognized ListButtonEvent [{0}] in OnGUI. Add a case to handle this event.",
                            buttonClickEvent));
                        return;
                    }
                }

                if (operationToFocus != null) {
                    FocusRenameOperationDeferred(operationToFocus);
                }

                if (saveOpsToPreferences) {
                    SaveUserPreferences();
                }
            }
        }

        private void OnAddRenameOperationConfirmed(object operation) {
            var operationDrawerBinding = operation as RenameOperationDrawerBinding;
            if (operationDrawerBinding == null) {
                throw new System.ArgumentException(
                    $"MulliganRenamerWindow tried to add a new RenameOperation using a type that is not a subclass of RenameOperationDrawerBinding. Operation type: {operation.GetType()}");
            }

            AddRenameOperation(operationDrawerBinding);
        }

        private void AddRenameOperation(RenameOperationDrawerBinding prototypeBinding) {
            // Reconstruct the operation and drawer so we are working with new instances
            var renameOp = prototypeBinding.Operation.Clone();
            var drawer = (IRenameOperationDrawer) System.Activator.CreateInstance(prototypeBinding.Drawer.GetType());
            drawer.SetModel(renameOp);

            var binding = new RenameOperationDrawerBinding(renameOp, drawer);
            RenameOperationsToApplyWithBindings.Add(binding);

            SaveUserPreferences();

            // Scroll to the bottom to focus the newly created operation.
            ScrollRenameOperationsToBottom();

            FocusRenameOperationDeferred(renameOp);
        }

        private void DrawPreviewPanel(Rect previewPanelRect, BulkRenamePreview bulkRenamePreview) {
            // PreviewPanel goes null when we recompile while the window is open
            if (previewPanel == null) {
                InitializePreviewPanel();
            }

            previewPanel.NumPreviouslyRenamedObjects = NumPreviouslyRenamedObjects;

            // If we aren't doing stepwise preview, send an invalid prefix so that the panel only renders before and after
            var previewIndex = IsShowingPreviewSteps ? FocusedRenameOpIndex : -1;
            previewPanel.PreviewStepIndexToShow = previewIndex;

            MulliganRenamerPreviewPanel.ColumnStyle columnStyle = MulliganRenamerPreviewPanel.ColumnStyle.OriginalAndFinalOnly;
            if (NumRenameOperations <= 1) {
                columnStyle = MulliganRenamerPreviewPanel.ColumnStyle.StepwiseHideFinal;
            }
            else if (IsShowingPreviewSteps) {
                columnStyle = MulliganRenamerPreviewPanel.ColumnStyle.Stepwise;
            }
            else {
                columnStyle = MulliganRenamerPreviewPanel.ColumnStyle.OriginalAndFinalOnly;
            }

            previewPanel.ColumnsToShow = columnStyle;
            previewPanel.DisableAddSelectedObjectsButton = ValidSelectedObjects.Count == 0;
            previewPanelScrollPosition = previewPanel.Draw(previewPanelRect, previewPanelScrollPosition, bulkRenamePreview);
        }

        private void DrawReviewPrompt(Rect rect) {
            var reviewPrompt = string.Empty;
            Color color = Color.blue;
            if (ActivePreferences.HasConfirmedReviewPrompt) {
                color = new AddStringOperationDrawer().HighlightColor;
                if (RBPackageSettings.IsGitHubRelease) {
                    reviewPrompt = "<color=FFFFFFF>Thank you very much for supporting Mulligan!</color>";
                }
                else {
                    reviewPrompt = "<color=FFFFFFF>Thank you for reviewing Mulligan!</color>";
                }
            }
            else {
                color = new ReplaceNameOperationDrawer().HighlightColor;

                if (RBPackageSettings.IsGitHubRelease) {
                    reviewPrompt = "<color=FFFFFFF>Thank you for using Mulligan! " +
                                   "If you've found it useful, please consider supporting its development by " +
                                   "purchasing it from the Asset Store. Thanks!</color>";
                }
                else {
                    reviewPrompt = "<color=FFFFFFF>Thank you for purchasing Mulligan! " +
                                   "If you've found it useful, please consider leaving a review on the Asset Store. " +
                                   "The store is very competitive and every review helps to stand out. Thanks!</color>";
                }
            }

            DrawReviewBanner(rect, color, reviewPrompt, !ActivePreferences.HasConfirmedReviewPrompt);
        }

        private void DrawReviewBanner(Rect rect, Color color, string prompt, bool showButton) {
            var oldColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = oldColor;

            var reviewStyle = new GUIStyle(EditorStyles.largeLabel);
            reviewStyle.fontStyle = FontStyle.Bold;
            reviewStyle.alignment = TextAnchor.MiddleCenter;
            reviewStyle.wordWrap = true;
            reviewStyle.richText = true;

            var buttonRect = new Rect(rect);
            buttonRect.width = showButton ? 140.0f : 0.0f;
            buttonRect.height = 16.0f;
            var buttonPaddingLR = 10.0f;

            buttonRect.x = rect.width - (buttonRect.width + buttonPaddingLR);
            buttonRect.y += (rect.height * 0.5f) - (buttonRect.height * 0.5f);

            var labelRect = new Rect(rect);
            var labelPaddingL = 10.0f;
            labelRect.x += labelPaddingL;
            labelRect.width = (buttonRect.x - rect.x) - (buttonPaddingLR + labelPaddingL);

            GUI.Label(labelRect, prompt, reviewStyle);
            if (showButton && GUI.Button(buttonRect, "Open Asset Store")) {
                ActivePreferences.HasConfirmedReviewPrompt = true;
                Application.OpenURL("https://assetstore.unity.com/packages/slug/99843");

                // Set a flag to continue to show the banner for this session
                IsShowingThanksForReview = true;
            }
        }

        private void ShowSavePresetWindow() {
            // Don't let them have both preset management windows open at once because it gets weird.
            if (activePresetManagementWindow != null) {
                activePresetManagementWindow.Close();
            }

            var existingWindow = activeSavePresetWindow;
            var windowMinSize = new Vector2(250.0f, 48.0f);
            var savePresetPosition = new Rect(position);
            savePresetPosition.size = windowMinSize;
            savePresetPosition.x = position.x + (position.width / 2.0f);
            savePresetPosition.y = position.y + (position.height / 2.0f);
            activeSavePresetWindow =
                GetWindowWithRect<SavePresetWindow>(savePresetPosition, true, "Save Preset", true);
            activeSavePresetWindow.minSize = windowMinSize;
            activeSavePresetWindow.maxSize = new Vector2(windowMinSize.x * 2.0f, windowMinSize.y);
            activeSavePresetWindow.SetName(CurrentPresetName);
            activeSavePresetWindow.SetExistingPresetNames(ActivePreferences.PresetNames);

            // Only subscribe if it's a new, previously unopened window.
            if (existingWindow == null) {
                activeSavePresetWindow.PresetSaved += HandlePresetSaved;
            }
        }

        private void HandlePresetSaved(string presetName) {
            var savedPreset = SaveNewPresetFromCurrentOperations(presetName);
            LoadPreset(savedPreset);
        }

        private void ShowManagePresetsWindow() {
            // Don't let them have both preset management windows open at once because it gets weird.
            if (activeSavePresetWindow != null) {
                activeSavePresetWindow.Close();
            }

            var existingWindow = activePresetManagementWindow;
            activePresetManagementWindow = GetWindow<ManagePresetsWindow>(true, "Manage Presets", true);
            activePresetManagementWindow.PopulateWithPresets(ActivePreferences.SavedPresets);

            // Only subscribe if it's a new, previously unopened window.
            if (existingWindow == null) {
                activePresetManagementWindow.PresetsChanged += HandlePresetsChanged;
            }
        }

        private void HandlePresetsChanged(List<RenameSequencePreset> presets) {
            var presetCopies = new List<RenameSequencePreset>(presets.Count);
            foreach (var preset in presets) {
                var copySerialized = JsonUtility.ToJson(preset);
                var copy = JsonUtility.FromJson<RenameSequencePreset>(copySerialized);
                presetCopies.Add(copy);
            }

            ActivePreferences.SavedPresets = presetCopies;

            // Clear the current preset name if it no longer exists after they changed.
            // This way we don't write to a preset that doesn't exist (if we were to auto save changes back to the preset).
            // Also so we don't populate the Save As field with a name that's bogus.
            var currentPresetIndex = ActivePreferences.FindIndexOfSavedPresetWithName(CurrentPresetName);
            if (currentPresetIndex < 0) {
                CurrentPresetName = string.Empty;
            }
        }

        private void SaveUserPreferences() {
            var operationSequence = GetCurrentRenameOperationSequence();
            ActivePreferences.PreviousSequence = operationSequence;

            EditorPrefs.SetString(UserPreferencesPrefKey, JsonUtility.ToJson(ActivePreferences));
        }

        private void LoadUserPreferences() {
            var oldSerializedOps = EditorPrefs.GetString(RenameOpsEditorPrefsKey, string.Empty);
            if (!string.IsNullOrEmpty(oldSerializedOps)) {
                // Update operations to the new preferences
                ActivePreferences = new MulliganUserPreferences() {
                    PreviousSequence = RenameOperationSequence<IRenameOperation>.FromString(oldSerializedOps)
                };

                EditorPrefs.DeleteKey(RenameOpsEditorPrefsKey);
            }
            else {
                var serializedPreferences = EditorPrefs.GetString(UserPreferencesPrefKey, string.Empty);

                if (!string.IsNullOrEmpty(serializedPreferences)) {
                    var loadedPreferences = JsonUtility.FromJson<MulliganUserPreferences>(serializedPreferences);
                    ActivePreferences = loadedPreferences;
                }
                else {
                    ActivePreferences = new MulliganUserPreferences();
                }
            }

            LoadOperationSequence(ActivePreferences.PreviousSequence);
            var originPreset = ActivePreferences.FindSavedPresetWithName(ActivePreferences.LastUsedPresetName);
            if (originPreset != null) {
                CurrentPresetName = originPreset.Name;
            }
        }

        private void LoadPreset(RenameSequencePreset preset) {
            CurrentPresetName = preset.Name;
            ActivePreferences.LastUsedPresetName = preset.Name;
            LoadOperationSequence(preset.OperationSequence);
        }

        private void LoadOperationSequence(RenameOperationSequence<IRenameOperation> sequence) {
            RenameOperationsToApplyWithBindings = new List<RenameOperationDrawerBinding>();

            foreach (var op in sequence) {
                // Find the drawer that goes with this operation's type
                foreach (var drawerBinding in RenameOperationDrawerBindingPrototypes) {
                    if (drawerBinding.Operation.GetType() == op.GetType()) {
                        AddRenameOperation(new RenameOperationDrawerBinding(op, drawerBinding.Drawer));
                        break;
                    }
                }
            }

            if (NumRenameOperations > 0) {
                FocusRenameOperationDeferred(RenameOperationsToApplyWithBindings.First().Operation);
            }
        }

        private RenameSequencePreset SaveNewPresetFromCurrentOperations(string presetName) {
            var preset = CreatePresetFromCurrentSequence(presetName);
            ActivePreferences.SavePreset(preset);

            return preset;
        }

        private RenameSequencePreset CreatePresetFromCurrentSequence(string presetName) {
            var operationSequence = GetCurrentRenameOperationSequence();
            var preset = new RenameSequencePreset() {
                Name = presetName,
                OperationSequence = operationSequence
            };

            return preset;
        }

        private RenameOperationSequence<IRenameOperation> GetCurrentRenameOperationSequence() {
            var operationSequence = new RenameOperationSequence<IRenameOperation>();
            foreach (var binding in RenameOperationsToApplyWithBindings) {
                var clone = binding.Operation.Clone();
                operationSequence.Add(clone);
            }

            return operationSequence;
        }

        private void FocusRenameOperationDeferred(IRenameOperation renameOperation) {
            OperationToForceFocus = renameOperation;
        }

        private void FocusForcedFocusControl() {
            if (OperationToForceFocus == null) {
                return;
            }

            var controlNameToForceFocus = string.Empty;
            for (int i = 0; i < NumRenameOperations; ++i) {
                var binding = RenameOperationsToApplyWithBindings[i];
                if (binding.Operation == OperationToForceFocus) {
                    controlNameToForceFocus = GUIControlNameUtility.CreatePrefixedName(i, binding.Drawer.ControlToFocus);
                    break;
                }
            }

            if (!string.IsNullOrEmpty(controlNameToForceFocus)) {
                var previouslyFocusedControl = GUI.GetNameOfFocusedControl();

                // Try to focus the desired control
                GUI.FocusControl(controlNameToForceFocus);
                EditorGUI.FocusTextInControl(controlNameToForceFocus);

                // Stop focusing the desired control only once it's been focused.
                // (Workaround because for some reason this fails to focus a control when users click between breadcrumbs)
                var focusedControl = GUI.GetNameOfFocusedControl();
                if (controlNameToForceFocus.Equals(focusedControl)) {
                    FocusRenameOperationDeferred(null);
                }
                else {
                    // If we weren't able to focus the new control, go back to whatever was focused before.
                    GUI.FocusControl(previouslyFocusedControl);
                    EditorGUI.FocusTextInControl(previouslyFocusedControl);
                }
            }
        }

        private void LoadSelectedObjects() {
            AddObjectsToRename(ValidSelectedObjects);

            // Scroll to the bottom to focus the newly added objects.
            ScrollPreviewPanelToBottom();
        }

        private void AddObjectsToRename(ICollection<Object> objectsToAdd) {
            // Sort the objects before adding them
            var assets = new List<Object>();
            var gameObjects = new List<Object>();
            foreach (var obj in objectsToAdd) {
                if (obj.IsAsset()) {
                    assets.Add(obj);
                }
                else {
                    gameObjects.Add((GameObject) obj);
                }
            }

            // When clicking and dragging from the scene, GameObjects are properly sorted according to the hierarchy.
            // But when selected and adding them, they are not. So we need to resort them here.
            gameObjects.Sort((x, y) => ((GameObject) x).GetHierarchySorting().CompareTo(((GameObject) y).GetHierarchySorting()));

            assets.Sort((x, y) => { return EditorUtility.NaturalCompare(x.name, y.name); });

            ObjectsToRename.AddRange(assets);
            ObjectsToRename.AddRange(gameObjects);

            // Reset the number of previously renamed objects so that we don't show the success prompt if these are removed.
            NumPreviouslyRenamedObjects = 0;
        }

        private bool ObjectIsValidForRename(Object obj) {
            return ObjectIsRenamable(obj) && !ObjectsToRename.Contains(obj);
        }

        private bool RenameOperatationsHaveErrors() {
            foreach (var binding in RenameOperationsToApplyWithBindings) {
                if (binding.Operation.HasErrors()) {
                    return true;
                }
            }

            return false;
        }

        private void ScrollPreviewPanelToBottom() {
            previewPanelScrollPosition = new Vector2(0.0f, 100000);
        }

        private void ScrollRenameOperationsToBottom() {
            renameOperationsPanelScrollPosition = new Vector2(0.0f, 100000);
        }

        private struct PreviewBreadcrumbOptions {
            public string Heading { get; set; }

            public Color32 HighlightColor { get; set; }

            public bool UseLeftStyle { get; set; }

            public bool Enabled { get; set; }

            public string StyleName {
                get { return UseLeftStyle ? "GUIEditor.BreadcrumbLeft" : "GUIEditor.BreadcrumbMid"; }
            }

            public Vector2 SizeForContent {
                get {
                    var style = new GUIStyle(StyleName);
                    return style.CalcSize(new GUIContent(Heading, string.Empty));
                }
            }
        }

        private class RenameOperationDrawerBinding {
            public IRenameOperation Operation { get; private set; }

            public IRenameOperationDrawer Drawer { get; private set; }

            public RenameOperationDrawerBinding(IRenameOperation operation, IRenameOperationDrawer drawer) {
                Operation = operation;
                Drawer = drawer;
            }
        }

        private class GUIStyles {
            public GUIStyle CopyrightLabel { get; set; }
        }

        private class GUIContents {
            public GUIContent CopyrightLabel { get; set; }
        }
    }
}