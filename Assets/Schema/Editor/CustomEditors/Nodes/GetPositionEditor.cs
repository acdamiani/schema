using Schema.Builtin.Nodes;
using UnityEditor;

namespace SchemaEditor.Editors.Nodes
{
    [CustomEditor(typeof(GetPosition)), CanEditMultipleObjects]
    public class GetPositionEditor : Editor
    {
        private SerializedProperty local;
        private SerializedProperty positionKey;
        private SerializedProperty transform;

        private void OnEnable()
        {
            transform = serializedObject.FindProperty("transform");
            local = serializedObject.FindProperty("local");
            positionKey = serializedObject.FindProperty("positionKey");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(transform);
            EditorGUILayout.PropertyField(local);
            EditorGUILayout.PropertyField(positionKey);

            serializedObject.ApplyModifiedProperties();
        }
    }
}