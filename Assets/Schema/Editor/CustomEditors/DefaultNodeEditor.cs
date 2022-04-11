using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(Schema.Node)), CanEditMultipleObjects]
public class DefaultNodeEditor : Editor
{
    SerializedProperty nodeName;
    SerializedProperty enableStatusIndicator;
    SerializedProperty comment;
    void OnEnable()
    {
        if (targets.Length > 0 && targets.All(obj => obj != null) && serializedObject != null)
        {
            try
            {
                nodeName = serializedObject.FindProperty("m_Name");
                comment = serializedObject.FindProperty("m_comment");
                enableStatusIndicator = serializedObject.FindProperty("m_enableStatusIndicator");
            }
            catch { }
        }
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