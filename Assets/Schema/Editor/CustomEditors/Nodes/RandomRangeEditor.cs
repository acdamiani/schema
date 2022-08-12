using System;
using Schema.Builtin.Nodes;
using Schema.Internal;
using UnityEditor;
using UnityEngine;

namespace SchemaEditor.Editors.Nodes
{
    [CustomEditor(typeof(RandomRange))]
    [CanEditMultipleObjects]
    public class RangeEditor : Editor
    {
        private SerializedProperty floatMax;
        private SerializedProperty floatMin;
        private SerializedProperty intMax;
        private SerializedProperty intMin;
        private float ma;
        private float mi;
        private SerializedProperty selector;

        private void OnEnable()
        {
            selector = serializedObject.FindProperty("target");
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
            bool isDynamic = selector.FindPropertyRelative("m_isDynamic").boolValue;

            if (entry == null && !isDynamic)
            {
                serializedObject.ApplyModifiedProperties();
                return;
            }

            if (isDynamic)
            {
                EditorGUILayout.PropertyField(floatMin, new GUIContent("Min"));
                EditorGUILayout.PropertyField(floatMax, new GUIContent("Max"));
            }
            else
            {
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
            }


            serializedObject.ApplyModifiedProperties();
        }
    }
}