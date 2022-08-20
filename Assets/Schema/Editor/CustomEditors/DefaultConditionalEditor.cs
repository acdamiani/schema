using System.Linq;
using Schema;
using UnityEditor;

namespace SchemaEditor.Editors
{
    [CustomEditor(typeof(Conditional)), CanEditMultipleObjects]
    public class DefaultConditionalEditor : Editor
    {
        private SerializedProperty abortsType;
        private SerializedProperty abortsWhen;
        private SerializedProperty conditionalName;
        private SerializedProperty invert;

        private void OnEnable()
        {
            if (targets.Any(x => x == null))
            {
                DestroyImmediate(this);
                return;
            }

            conditionalName = serializedObject.FindProperty("m_Name");
            invert = serializedObject.FindProperty("m_invert");
            abortsType = serializedObject.FindProperty("m_abortsType");
            abortsWhen = serializedObject.FindProperty("m_abortsWhen");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(conditionalName);
            EditorGUILayout.PropertyField(invert);
            EditorGUILayout.PropertyField(abortsType);

            if (abortsType.intValue != 0)
                EditorGUILayout.PropertyField(abortsWhen);

            serializedObject.ApplyModifiedProperties();
        }
    }
}