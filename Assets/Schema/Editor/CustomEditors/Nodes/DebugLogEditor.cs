using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DebugLog)), CanEditMultipleObjects]
public class DebugLogEditor : Editor
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
        DebugLog debugLog = (DebugLog)target;

        if (boxStyle == null)
        {
            boxStyle = new GUIStyle(EditorStyles.helpBox);
            boxStyle.richText = true;
        }

        string[] names = new string[0];
        if (debugLog != null && debugLog.keys != null)
        {
            names = debugLog.keys.Select(key =>
            {
                BlackboardEntrySelectorDrawer.names.TryGetValue(key.entryID, out string name);

                return name;
            }).ToArray();
        }

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