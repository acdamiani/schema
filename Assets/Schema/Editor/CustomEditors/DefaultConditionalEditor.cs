using System;
using System.Collections.Generic;
using System.Linq;
using Schema;
using Schema.Utilities;
using UnityEditor;
using UnityEngine;

namespace SchemaEditor.Editors
{
    [CustomEditor(typeof(Conditional)), CanEditMultipleObjects]
    public class DefaultConditionalEditor : Editor
    {
        private SerializedProperty conditionalName;
        private SerializedProperty invert;

        private void OnEnable()
        {
            conditionalName = serializedObject.FindProperty("m_Name");
            invert = serializedObject.FindProperty("m_invert");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(conditionalName);
            EditorGUILayout.PropertyField(invert);

            serializedObject.ApplyModifiedProperties();
        }

    }
}