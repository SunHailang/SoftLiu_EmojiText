using System.Collections;
using System.Collections.Generic;
using igg.EmojiText.Runtime;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;


[CustomEditor(typeof(OutLineText), true)]
[CanEditMultipleObjects]
public class OutLineTextEditor : GraphicEditor
{
    #region 属性
    SerializedProperty m_Text;
    SerializedProperty m_FontData;
    
    SerializedProperty m_OutLine ;
    SerializedProperty m_OutLineColor;
    SerializedProperty m_OutLineOffset;
    #endregion
    protected override void OnEnable()
    {
        base.OnEnable();
        m_Text = serializedObject.FindProperty("m_Text");
        m_FontData = serializedObject.FindProperty("m_FontData");
        
        m_OutLine = serializedObject.FindProperty("m_OutLine");
        m_OutLineColor = serializedObject.FindProperty("m_OutLineColor");
        m_OutLineOffset = serializedObject.FindProperty("m_OutLineOffset");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_OutLine);
        EditorGUILayout.PropertyField(m_OutLineColor);
        EditorGUILayout.PropertyField(m_OutLineOffset);
        
        EditorGUILayout.PropertyField(m_Text);
        EditorGUILayout.PropertyField(m_FontData);
            
        AppearanceControlsGUI();
        RaycastControlsGUI();
        serializedObject.ApplyModifiedProperties();
    }
}