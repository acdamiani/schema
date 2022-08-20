using Schema.Builtin.Conditionals;
using UnityEditor;
using UnityEngine;

namespace SchemaEditor.Editors.Nodes
{
    [CustomEditor(typeof(ConeCheck)), CanEditMultipleObjects]
    public class ConeCheckEditor : Editor
    {
        private SerializedProperty coneType;
        private SerializedProperty direction;
        private SerializedProperty gameObjectKey;
        private SerializedProperty halfAngle;
        private SerializedProperty offset;
        private SerializedProperty rayRange;
        private SerializedProperty resolution;
        private int tab;
        private SerializedProperty tagFilter;
        private SerializedProperty visualize;

        private void OnEnable()
        {
            coneType = serializedObject.FindProperty("precisionMode");
            halfAngle = serializedObject.FindProperty("halfAngle");
            tagFilter = serializedObject.FindProperty("tagFilter");
            resolution = serializedObject.FindProperty("resolution");
            rayRange = serializedObject.FindProperty("rayRange");
            offset = serializedObject.FindProperty("offset");
            direction = serializedObject.FindProperty("coneDirection");
            gameObjectKey = serializedObject.FindProperty("gameObjectKey");
            visualize = serializedObject.FindProperty("visualize");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            tab = GUILayout.Toolbar(coneType.boolValue ? 1 : 0, new[] { "Efficient", "Precise" });
            coneType.boolValue = tab == 1;

            EditorGUILayout.PropertyField(visualize);

            EditorGUILayout.LabelField("Cone properties", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(tagFilter);
            EditorGUILayout.PropertyField(gameObjectKey);

            EditorGUILayout.PropertyField(halfAngle);
            EditorGUILayout.PropertyField(rayRange);
            EditorGUILayout.PropertyField(offset);
            EditorGUILayout.PropertyField(direction);

            if (coneType.boolValue) EditorGUILayout.PropertyField(resolution);

            serializedObject.ApplyModifiedProperties();
        }
    }
}