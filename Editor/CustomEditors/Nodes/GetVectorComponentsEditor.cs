using System.Collections.Generic;
using Schema.Builtin.Nodes;
using UnityEditor;
using UnityEngine;

namespace SchemaEditor.Editors.Nodes
{
    [CustomEditor(typeof(GetVectorComponents)), CanEditMultipleObjects]
    public class GetVectorComponentsEditor : Editor
    {
        private readonly Dictionary<Object, GetVectorComponents>
            vectors = new Dictionary<Object, GetVectorComponents>();

        private SerializedProperty vector;
        private SerializedProperty w;
        private SerializedProperty x;
        private SerializedProperty y;
        private SerializedProperty z;

        private void OnEnable()
        {
            x = serializedObject.FindProperty("x");
            y = serializedObject.FindProperty("y");
            z = serializedObject.FindProperty("z");
            w = serializedObject.FindProperty("z");
            vector = serializedObject.FindProperty("vector");

            for (int i = 0; i < targets.Length; i++)
                vectors[targets[i]] = (GetVectorComponents)targets[i];
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(vector);

            EditorGUILayout.PropertyField(x);
            EditorGUILayout.PropertyField(y);

            if (vectors[target]?.vector.entryType == typeof(Vector3))
            {
                EditorGUILayout.PropertyField(z);
            }
            else if (vectors[target]?.vector.entryType == typeof(Vector4))
            {
                EditorGUILayout.PropertyField(w);
                EditorGUILayout.PropertyField(z);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}