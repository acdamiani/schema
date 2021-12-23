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

        Type t = Type.GetType(entry.type);
        switch (Type.GetTypeCode(t))
        {
            case TypeCode.String:
                stringValue.stringValue = EditorGUILayout.TextField("Value", stringValue.stringValue);
                break;
            case TypeCode.Int32:
                intValue.intValue = EditorGUILayout.IntField("Value", intValue.intValue);
                break;
            case TypeCode.Single:
                floatValue.floatValue = EditorGUILayout.FloatField("Value", floatValue.floatValue);
                break;
            default:
                if (typeof(Vector4) == t)
                    vector4Value.vector4Value = EditorGUILayout.Vector4Field("Value", vector4Value.vector4Value);
                else if (typeof(Vector3) == t)
                    vector3Value.vector3Value = EditorGUILayout.Vector3Field("Value", vector3Value.vector3Value);
                else if (typeof(Vector2) == t)
                    vector2Value.vector2Value = EditorGUILayout.Vector2Field("Value", vector2Value.vector2Value);

                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}