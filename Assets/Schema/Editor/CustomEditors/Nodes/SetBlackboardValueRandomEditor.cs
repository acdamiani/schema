using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SetBlackboardValueRandom)), CanEditMultipleObjects]
public class SetBlackboardValueRandomEditor : Editor
{
    SerializedProperty selector;
    SerializedProperty intMax;
    SerializedProperty intMin;
    SerializedProperty floatMax;
    SerializedProperty floatMin;
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
        BlackboardEntry entry = Blackboard.instance.GetEntry(selector.FindPropertyRelative("entryID").stringValue);

        if (entry == null) return;

        GUILayout.BeginHorizontal();

        Type t = entry.type;
        switch (Type.GetTypeCode(t))
        {
            case TypeCode.Single:
                GUILayout.Label("Min");
                floatMin.floatValue = EditorGUILayout.FloatField(floatMin.floatValue);
                GUILayout.Label("Max");
                floatMax.floatValue = EditorGUILayout.FloatField(floatMax.floatValue);
                break;
            case TypeCode.Int32:
                GUILayout.Label("Min");
                intMin.intValue = EditorGUILayout.IntField(intMin.intValue);
                GUILayout.Label("Max");
                intMax.intValue = EditorGUILayout.IntField(intMax.intValue);
                break;
        }

        GUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }
}