using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Toolbox.ValueAsset {
    [CustomEditor(typeof(ValueAsset<>))]
    public class ValueAssetEditor : Editor {
        
        const string ResourceFilename = "custom-editor-uie";
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement customInspector = new VisualElement();
            var visualTree = Resources.Load(ResourceFilename) as VisualTreeAsset;
            visualTree.CloneTree(customInspector);
            customInspector.styleSheets.Add(Resources.Load($"{ResourceFilename}-style") as StyleSheet);
            return customInspector;
        }
    }
}