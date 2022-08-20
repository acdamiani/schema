using Schema.Builtin.Nodes;
using UnityEditor;
using UnityEngine;

namespace SchemaEditor.Editors.Nodes
{
    [CustomEditor(typeof(MoveToRandom)), CanEditMultipleObjects]
    public class MoveToRandomEditor : Editor
    {
        private SerializedProperty isRelative;
        private SerializedProperty speed;
        private SerializedProperty x;
        private SerializedProperty y;
        private SerializedProperty z;

        private void OnEnable()
        {
            x = serializedObject.FindProperty("x");
            y = serializedObject.FindProperty("y");
            z = serializedObject.FindProperty("z");
            speed = serializedObject.FindProperty("speed");
            isRelative = serializedObject.FindProperty("isRelative");
        }

        public override void OnInspectorGUI()
        {
            Undo.RecordObjects(targets, "Inspector");
            serializedObject.Update();

            EditorGUILayout.LabelField("X");
            DrawMinMax(x);
            EditorGUILayout.LabelField("Y");
            DrawMinMax(y);
            EditorGUILayout.LabelField("Z");
            DrawMinMax(z);

            EditorGUILayout.PropertyField(speed);
            EditorGUILayout.PropertyField(isRelative);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawMinMax(SerializedProperty property)
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label("Min");
            float xMin = EditorGUILayout.FloatField(property.vector2Value.x);
            GUILayout.Label("Max");
            float xMax = EditorGUILayout.FloatField(property.vector2Value.y);

            property.vector2Value = new Vector2(xMin, xMax);

            GUILayout.EndHorizontal();
        }
    }
}