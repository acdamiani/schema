using Schema.Builtin.Nodes;
using UnityEditor;
using UnityEngine;

namespace SchemaEditor.Editors.Nodes
{
    [CustomEditor(typeof(SetAnimatorVariable)), CanEditMultipleObjects]
    public class SetAnimatorVariableEditor : Editor
    {
        private SerializedProperty animator;
        private SerializedProperty boolValue;
        private SerializedProperty floatValue;
        private SerializedProperty intValue;
        private SerializedProperty parameterName;
        private SerializedProperty type;

        private void OnEnable()
        {
            animator = serializedObject.FindProperty("animator");
            type = serializedObject.FindProperty("type");
            parameterName = serializedObject.FindProperty("parameterName");
            floatValue = serializedObject.FindProperty("floatValue");
            intValue = serializedObject.FindProperty("intValue");
            boolValue = serializedObject.FindProperty("boolValue");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(animator);
            EditorGUILayout.PropertyField(type);
            EditorGUILayout.PropertyField(parameterName);

            switch (type.enumValueIndex)
            {
                case 0:
                    EditorGUILayout.PropertyField(floatValue, new GUIContent("\0"));
                    break;
                case 1:
                    EditorGUILayout.PropertyField(intValue, new GUIContent("\0"));
                    break;
                case 2:
                    EditorGUILayout.PropertyField(boolValue, new GUIContent("\0"));
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}