using Schema.Builtin.Nodes;
using UnityEditor;

namespace SchemaEditor.Editors.Nodes
{
    [CustomEditor(typeof(SetAnimatorIKPositionWeight)), CanEditMultipleObjects]
    public class SetAnimatorIKPositionWeightEditor : Editor
    {
        private SerializedProperty animator;
        private SerializedProperty goal;
        private SerializedProperty hint;
        private SerializedProperty isHint;
        private SerializedProperty weight;

        private void OnEnable()
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
}