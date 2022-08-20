using Schema.Builtin.Nodes;
using UnityEditor;
using UnityEngine;

namespace SchemaEditor.Editors.Nodes
{
    [CustomEditor(typeof(SetAnimatorIKPosition)), CanEditMultipleObjects]
    public class SetAnimatorIKPositionEditor : Editor
    {
        private SerializedProperty animator;
        private SerializedProperty goal;
        private SerializedProperty hint;
        private SerializedProperty isHint;
        private SerializedProperty position;
        private SerializedProperty rotation;

        private void OnEnable()
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

            EditorGUILayout.PropertyField(position,
                new GUIContent(isHint.boolValue ? "Hint Position" : "Goal Position"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}