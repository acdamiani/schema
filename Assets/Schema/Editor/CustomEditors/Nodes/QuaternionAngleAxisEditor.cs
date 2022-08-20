using Schema.Builtin.Nodes;
using UnityEditor;

namespace SchemaEditor.Editors.Nodes
{
    [CustomEditor(typeof(QuaternionAngleAxis)), CanEditMultipleObjects]
    public class QuaternionAngleAxisEditor : Editor
    {
        private SerializedProperty a;
        private SerializedProperty angle;
        private SerializedProperty axis;
        private SerializedProperty dir;
        private SerializedProperty overrideAxis;

        private void OnEnable()
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