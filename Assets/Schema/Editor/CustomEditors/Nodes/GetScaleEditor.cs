using UnityEngine;
using UnityEditor;

namespace SchemaEditor.Editors.Nodes
{
    [CustomEditor(typeof(GetScale)), CanEditMultipleObjects]
    public class GetScaleEditor : Editor
    {
        SerializedProperty useSelf;
        SerializedProperty gameObjectKey;
        SerializedProperty positionKey;
        SerializedProperty local;
        void OnEnable()
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