using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Schema.Builtin.Nodes.SetBlackboardValueRandom)), CanEditMultipleObjects]
public class SetBlackboardValueRandomEditor : Editor
{
    SerializedProperty selector;
    SerializedProperty intMax;
    SerializedProperty intMin;
    SerializedProperty floatMax;
    SerializedProperty floatMin;
    float mi;
    float ma;
    void OnEnable()
    {
        selector = serializedObject.FindProperty("selector");
        intMax = serializedObject.FindProperty("intMax");
        floatMax = serializedObject.FindProperty("floatMax");
        intMin = serializedObject.FindProperty("intMin");
        floatMin = serializedObject.FindProperty("floatMin");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(selector);
        BlackboardEntry entry = (BlackboardEntry)selector.FindPropertyRelative("m_entry").objectReferenceValue;

        if (entry == null) return;


        Type t = entry.type;
        switch (Type.GetTypeCode(t))
        {
            case TypeCode.Single:
                EditorGUILayout.PropertyField(floatMin, new GUIContent("Min"));
                EditorGUILayout.PropertyField(floatMax, new GUIContent("Max"));
                break;
            case TypeCode.Int32:
                EditorGUILayout.PropertyField(intMin, new GUIContent("Min"));
                EditorGUILayout.PropertyField(intMax, new GUIContent("Max"));
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}