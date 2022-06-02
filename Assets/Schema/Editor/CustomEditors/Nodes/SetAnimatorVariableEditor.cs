using UnityEngine;
using UnityEditor;

namespace SchemaEditor.Editors.Nodes
{
    [CustomEditor(typeof(SetAnimatorVariable)), CanEditMultipleObjects]
    public class SetAnimatorVariableEditor : Editor
    {
        SerializedProperty animator;
        SerializedProperty type;
        SerializedProperty parameterName;
        SerializedProperty floatValue;
        SerializedProperty intValue;
        SerializedProperty boolValue;

        void OnEnable()
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