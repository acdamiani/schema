using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SetAnimatorIKPositionWeight)), CanEditMultipleObjects]
public class SetAnimatorIKPositionWeightEditor : Editor
{
    SerializedProperty isHint;
    SerializedProperty animator;
    SerializedProperty hint;
    SerializedProperty goal;
    SerializedProperty weight;
    void OnEnable()
    {
        animator = serializedObject.FindProperty("animator");
        isHint = serializedObject.FindProperty("isHint");
        hint = serializedObject.FindProperty("hint");
        goal = serializedObject.FindProperty("goal");
        weight = serializedObject.FindProperty("weight");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(animator);
        EditorGUILayout.PropertyField(isHint);

        if (isHint.boolValue)
            EditorGUILayout.PropertyField(hint);
        else
            EditorGUILayout.PropertyField(goal);

        EditorGUILayout.PropertyField(weight);

        serializedObject.ApplyModifiedProperties();
    }
}