using Schema.Builtin.Nodes;
using UnityEditor;

namespace SchemaEditor.Editors.Nodes
{
    [CustomEditor(typeof(GetRotation)), CanEditMultipleObjects]
    public class GetRotationEditor : Editor
    {
        private SerializedProperty eulerAngles;
        private SerializedProperty eulerKey;
        private SerializedProperty gameObjectKey;
        private SerializedProperty quaternionKey;
        private SerializedProperty useSelf;

        private void OnEnable()
        {
            useSelf = serializedObject.FindProperty("useSelf");
            eulerAngles = serializedObject.FindProperty("eulerAngles");
            gameObjectKey = serializedObject.FindProperty("gameObject");
            eulerKey = serializedObject.FindProperty("eulerKey");
            quaternionKey = serializedObject.FindProperty("quaternionKey");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(useSelf);

            if (!useSelf.boolValue)
                EditorGUILayout.PropertyField(gameObjectKey);

            EditorGUILayout.PropertyField(eulerAngles);

            if (eulerAngles.boolValue)
                EditorGUILayout.PropertyField(eulerKey);
            else
                EditorGUILayout.PropertyField(quaternionKey);

            serializedObject.ApplyModifiedProperties();
        }
    }
}