using Schema.Builtin.Nodes;
using UnityEditor;

namespace SchemaEditor.Editors.Nodes
{
    [CustomEditor(typeof(GetScale)), CanEditMultipleObjects]
    public class GetScaleEditor : Editor
    {
        private SerializedProperty gameObjectKey;
        private SerializedProperty local;
        private SerializedProperty positionKey;
        private SerializedProperty useSelf;

        private void OnEnable()
        {
            useSelf = serializedObject.FindProperty("useSelf");
            gameObjectKey = serializedObject.FindProperty("gameObject");
            positionKey = serializedObject.FindProperty("scaleKey");
            local = serializedObject.FindProperty("local");
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