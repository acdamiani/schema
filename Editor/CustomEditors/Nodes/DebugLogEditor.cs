using Schema.Builtin.Nodes;
using UnityEditor;
using UnityEngine;

namespace SchemaEditor.Editors.Nodes
{
    [CustomEditor(typeof(DebugLog)), CanEditMultipleObjects]
    public class DebugLogEditor : Editor
    {
        private GUIStyle boxStyle;
        private SerializedProperty logType;
        private SerializedProperty message;

        private void OnEnable()
        {
            message = serializedObject.FindProperty("message");
            logType = serializedObject.FindProperty("logType");
        }

        public override void OnInspectorGUI()
        {
            DebugLog debugLog = (DebugLog)target;

            if (boxStyle == null)
            {
                boxStyle = new GUIStyle(EditorStyles.helpBox);
                boxStyle.richText = true;
            }

            serializedObject.Update();

            EditorGUILayout.PropertyField(message);
            EditorGUILayout.PropertyField(logType);

            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
            EditorGUILayout.SelectableLabel(message.stringValue, boxStyle);

            serializedObject.ApplyModifiedProperties();
        }
    }
}