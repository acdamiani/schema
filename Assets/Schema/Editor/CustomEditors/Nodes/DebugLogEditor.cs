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
        DebugLog debugLog = (DebugLog)targets[0];

        if (boxStyle == null)
        {
            boxStyle = new GUIStyle(EditorStyles.helpBox);
            boxStyle.richText = true;
        }

        string[] names = new string[0];
        if (debugLog != null && debugLog.keys != null)
            names = debugLog.keys.Select(key => key.valuePath).ToArray();

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
        EditorGUILayout.TextArea(m, boxStyle);

        serializedObject.ApplyModifiedProperties();
    }
}