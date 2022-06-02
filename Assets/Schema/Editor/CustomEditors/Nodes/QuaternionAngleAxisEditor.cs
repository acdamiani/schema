using System;
using System.Collections.Generic;
using System.Linq;
using Schema.Builtin.Nodes;
using UnityEngine;
using UnityEditor;

namespace SchemaEditor.Editors.Nodes
{
    [CustomEditor(typeof(QuaternionAngleAxis)), CanEditMultipleObjects]
    public class QuaternionAngleAxisEditor : Editor
    {
        SerializedProperty a;
        SerializedProperty axis;
        SerializedProperty angle;
        SerializedProperty overrideAxis;
        SerializedProperty dir;
        void OnEnable()
        {
            axis = serializedObject.FindProperty("axis");
            angle = serializedObject.FindProperty("angle");
            overrideAxis = serializedObject.FindProperty("overrideAxis");
            dir = serializedObject.FindProperty("direction");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(angle);
            EditorGUILayout.PropertyField(overrideAxis);

            if (overrideAxis.boolValue)
                EditorGUILayout.PropertyField(axis);
            else
                EditorGUILayout.PropertyField(dir);

            serializedObject.ApplyModifiedProperties();
        }
    }
}