using System;
using System.Linq;
using Schema.Builtin.Nodes;
using UnityEditor;
using UnityEngine;

namespace SchemaEditor.Editors.Nodes
{
    [CustomEditor(typeof(DebugLogFormat)), CanEditMultipleObjects]
    public class DebugLogFormatEditor : Editor
    {
        private GUIStyle boxStyle;
        private SerializedProperty keys;
        private string m;
        private SerializedProperty message;

        private void OnEnable()
        {
            message = serializedObject.FindProperty("message");
            keys = serializedObject.FindProperty("keys");
        }

        public override void OnInspectorGUI()
        {
            DebugLogFormat debugLog = (DebugLogFormat)target;

            if (boxStyle == null)
            {
                boxStyle = new GUIStyle(EditorStyles.helpBox);
                boxStyle.richText = true;
            }

            string[] names = null;

            if (debugLog != null && debugLog.keys != null)
                names = debugLog.keys.Select(key => key.name).ToArray();

            serializedObject.Update();

            EditorGUILayout.PropertyField(message);
            EditorGUILayout.PropertyField(keys);

            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);

            try
            {
                m = string.Format(message.stringValue, names);
            }
            catch (Exception e)
            {
                EditorGUILayout.HelpBox(e.Message, MessageType.Warning);
            }

            EditorGUILayout.SelectableLabel(m, boxStyle);

            serializedObject.ApplyModifiedProperties();
        }
    }
}