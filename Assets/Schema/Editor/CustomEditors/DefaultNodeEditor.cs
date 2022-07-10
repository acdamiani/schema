using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(Schema.Node)), CanEditMultipleObjects]
public class DefaultNodeEditor : Editor
{
    SerializedProperty nodeName;
    SerializedProperty enableStatusIndicator;
    SerializedProperty comment;
    SerializedProperty priority;
    void OnEnable()
    {
        nodeName = serializedObject.FindProperty("m_Name");
        comment = serializedObject.FindProperty("m_comment");
        enableStatusIndicator = serializedObject.FindProperty("m_enableStatusIndicator");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(nodeName);
        EditorGUILayout.PropertyField(comment);
        EditorGUILayout.PropertyField(enableStatusIndicator);

        serializedObject.ApplyModifiedProperties();
    }
}