using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

public class TestingEditor : EditorWindow
{
    [MenuItem("Tools/Testing Editor (UI Toolkit)")]
    public static void ShowWindow()
    {
        // Create and show the window
        TestingEditor window = GetWindow<TestingEditor>();
        window.titleContent = new GUIContent("Testing Editor");
    }

    private void OnEnable()
    {
        // Load UXML layout
        VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/_Project/Outdated/_TestingScripts/TestingEditor.uxml");
        VisualElement root = visualTree.CloneTree();
        rootVisualElement.Add(root);

        // Load USS stylesheet
        StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/_Project/Outdated/_TestingScripts/TestingEditor.uss");
        rootVisualElement.styleSheets.Add(styleSheet);

    }
}