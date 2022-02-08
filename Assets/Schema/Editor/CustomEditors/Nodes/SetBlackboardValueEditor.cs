using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SetBlackboardValue)), CanEditMultipleObjects]
public class SetBlackboardValueEditor : Editor
{
    SerializedProperty selector;
    SerializedProperty stringValue;
    SerializedProperty intValue;
    SerializedProperty floatValue;
    SerializedProperty vector4Value;
    SerializedProperty vector3Value;
    SerializedProperty vector2Value;
    private void OnEnable()
    {
        selector = serializedObject.FindProperty("selector");
        stringValue = serializedObject.FindProperty("stringValue");
        intValue = serializedObject.FindProperty("intValue");
        floatValue = serializedObject.FindProperty("floatValue");
        vector4Value = serializedObject.FindProperty("vector4Value");
        vector3Value = serializedObject.FindProperty("vector3Value");
        vector2Value = serializedObject.FindProperty("vector2Value");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(selector);

        Blackboard blackboard = ((SetBlackboardValue)targets[0]).graph.blackboard;
        BlackboardEntry entry = blackboard.GetEntry(selector.FindPropertyRelative("entryID").stringValue);

        if (entry == null) return;

        Type t = entry.type;
        switch (Type.GetTypeCode(t))
        {
            case TypeCode.String:
                EditorGUILayout.PropertyField(stringValue);
                break;
            case TypeCode.Int32:
                EditorGUILayout.PropertyField(intValue);
                break;
            case TypeCode.Single:
                EditorGUILayout.PropertyField(floatValue);
                break;
            default:
                if (typeof(Vector4) == t)
                    EditorGUILayout.PropertyField(vector4Value);
                else if (typeof(Vector3) == t)
                    EditorGUILayout.PropertyField(vector3Value);
                else if (typeof(Vector2) == t)
                    EditorGUILayout.PropertyField(vector2Value);

                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}