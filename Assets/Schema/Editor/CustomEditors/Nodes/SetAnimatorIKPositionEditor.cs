using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SetAnimatorIKPosition)), CanEditMultipleObjects]
public class SetAnimatorIKPositionEditor : Editor
{
    SerializedProperty isHint;
    SerializedProperty animator;
    SerializedProperty hint;
    SerializedProperty goal;
    SerializedProperty position;
    SerializedProperty rotation;
    void OnEnable()
    {
        animator = serializedObject.FindProperty("animator");
        isHint = serializedObject.FindProperty("isHint");
        hint = serializedObject.FindProperty("hint");
        goal = serializedObject.FindProperty("goal");
        position = serializedObject.FindProperty("goalPosition");
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

        EditorGUILayout.PropertyField(position, new GUIContent(isHint.boolValue ? "Hint Position" : "Goal Position"));

        serializedObject.ApplyModifiedProperties();
    }
}