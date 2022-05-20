using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DebugLogFormat)), CanEditMultipleObjects]
public class DebugLogFormatEditor : Editor
{
    SerializedProperty message;
    SerializedProperty keys;
    string m;
    GUIStyle boxStyle;
    void OnEnable()
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
            names = debugLog.keys.Select(key => key.entryName).ToArray();

        serializedObject.Update();

        EditorGUILayout.PropertyField(message);
        EditorGUILayout.PropertyField(keys);

        EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);

        try
        {
            m = String.Format(message.stringValue, names);
        }
        catch (Exception e)
        {
            EditorGUILayout.HelpBox(e.Message, MessageType.Warning);
        }
        EditorGUILayout.SelectableLabel(m, boxStyle);

        serializedObject.ApplyModifiedProperties();
    }
}