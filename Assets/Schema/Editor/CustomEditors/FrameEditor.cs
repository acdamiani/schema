using UnityEngine;
using UnityEditor;
using Schema;

namespace SchemaEditor.Editors
{
    [CustomEditor(typeof(Frame))]
    public class FrameEditor : Editor
    {
        SerializedProperty useCustomColor;
        SerializedProperty customColor;
        void OnEnable()
        {
            useCustomColor = serializedObject.FindProperty("m_useCustomColor");
            customColor = serializedObject.FindProperty("m_customColor");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(useCustomColor);

            EditorGUI.BeginDisabledGroup(!useCustomColor.boolValue);
            EditorGUILayout.PropertyField(customColor);
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }
    }
}