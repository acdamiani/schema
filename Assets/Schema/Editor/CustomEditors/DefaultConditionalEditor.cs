using Schema;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace SchemaEditor.Editors
{
    [CustomEditor(typeof(Conditional))]
    [CanEditMultipleObjects]
    public class DefaultConditionalEditor : Editor
    {
        private SerializedProperty conditionalName;
        private SerializedProperty invert;
        private SerializedProperty abortsType;

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
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(conditionalName);
            EditorGUILayout.PropertyField(invert);
            EditorGUILayout.PropertyField(abortsType);

            serializedObject.ApplyModifiedProperties();
        }
    }
}