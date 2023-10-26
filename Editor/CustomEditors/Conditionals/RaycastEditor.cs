using Schema.Builtin.Conditionals;
using UnityEditor;
using UnityEngine;

namespace SchemaEditor.Editors
{
    [CustomEditor(typeof(Raycast)), CanEditMultipleObjects]
    public class RaycastEditor : Editor
    {
        private SerializedProperty direction;
        private SerializedProperty maxDistance;
        private SerializedProperty offset;
        private SerializedProperty point;
        private SerializedProperty tagFilter;
        private SerializedProperty visualize;

        private void OnEnable()
        {
            offset = serializedObject.FindProperty("offset");
            direction = serializedObject.FindProperty("direction");
            point = serializedObject.FindProperty("point");
            tagFilter = serializedObject.FindProperty("tagFilter");
            maxDistance = serializedObject.FindProperty("maxDistance");
            visualize = serializedObject.FindProperty("visualize");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Raycast raycast = (Raycast)target;

            raycast.type = (Raycast.RaycastType)GUILayout.Toolbar((int)raycast.type, new[] { "Absolute", "Dynamic" });

            EditorGUILayout.PropertyField(visualize);

            EditorGUILayout.PropertyField(tagFilter);

            if (raycast.type == Raycast.RaycastType.Absolute)
            {
                EditorGUILayout.PropertyField(offset);
                EditorGUILayout.PropertyField(direction);
                EditorGUILayout.PropertyField(maxDistance);
            }
            else
            {
                EditorGUILayout.PropertyField(point);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
