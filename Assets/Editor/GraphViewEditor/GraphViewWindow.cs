using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.GraphViewEditor
{
    public class GraphViewWindow : EditorWindow
    {
        [MenuItem("Tools/GraphViewWindow")]
        public static void ShowExample()
        {
            //GraphViewWindow wnd = CreateWindow<GraphViewWindow>("GraphViewWindow");
            GraphViewWindow wnd = GetWindow<GraphViewWindow>();
            //wnd.minSize = new Vector2(500, 380);
            wnd.titleContent = new GUIContent("GraphViewWindow");
        }

        private InspectorGraphView m_InspectorGrapView = null;
        
        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Import UXML "Assets/Editor/GraphViewEditor/GraphViewWindow.uxml" 
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath("926e392990b30dc45894741fab0cfc3d"));
            visualTree.CloneTree(root);
            // VisualElement uxml = visualTree.CloneTree();
            // while (uxml.childCount > 0)
            // {
            //     root.Add(uxml.ElementAt(0));
            // }
            // root.Add(uxml);
            // "Assets/Editor/GraphViewEditor/GraphViewWindow.uss"
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath("b0fbc4ef7f2248849905faffbc7a8a9c"));
            root.styleSheets.Add(styleSheet);

            m_InspectorGrapView = root.Q<InspectorGraphView>("InspectorGraphView");
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }
    }
}