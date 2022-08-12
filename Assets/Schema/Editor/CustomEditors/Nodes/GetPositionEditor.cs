using Schema.Builtin.Nodes;
using UnityEditor;

namespace SchemaEditor.Editors.Nodes
{
    [CustomEditor(typeof(GetPosition))]
    [CanEditMultipleObjects]
    public class GetPositionEditor : Editor
    {
        private SerializedProperty gameObjectKey;
        private SerializedProperty local;
        private SerializedProperty positionKey;
        private SerializedProperty useSelf;

        private void OnEnable()
        {
            useSelf = serializedObject.FindProperty("useSelf");
            local = serializedObject.FindProperty("local");
            gameObjectKey = serializedObject.FindProperty("gameObject");
            positionKey = serializedObject.FindProperty("positionKey");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(useSelf);
            EditorGUILayout.PropertyField(local);

            if (!useSelf.boolValue)
                EditorGUILayout.PropertyField(gameObjectKey);

            EditorGUILayout.PropertyField(positionKey);

            serializedObject.ApplyModifiedProperties();
        }
    }
}