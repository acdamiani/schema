using Schema.Builtin.Nodes;
using UnityEditor;

namespace SchemaEditor.Editors.Nodes
{
    [CustomEditor(typeof(GetBounds)), CanEditMultipleObjects]
    public class GetBoundsEditor : Editor
    {
        private SerializedProperty boundsKey;
        private SerializedProperty gameObjectKey;
        private SerializedProperty useSelf;

        private void OnEnable()
        {
            useSelf = serializedObject.FindProperty("useSelf");
            gameObjectKey = serializedObject.FindProperty("gameObject");
            boundsKey = serializedObject.FindProperty("boundsKey");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(useSelf);

            if (!useSelf.boolValue)
                EditorGUILayout.PropertyField(gameObjectKey);

            EditorGUILayout.PropertyField(boundsKey);

            serializedObject.ApplyModifiedProperties();
        }
    }
}