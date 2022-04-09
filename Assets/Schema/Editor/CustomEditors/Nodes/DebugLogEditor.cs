using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DebugLog)), CanEditMultipleObjects]
public class DebugLogEditor : Editor
{
    SerializedProperty message;
    GUIStyle boxStyle;
    void OnEnable()
    {
        message = serializedObject.FindProperty("message");
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

        EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
        EditorGUILayout.SelectableLabel(message.stringValue, boxStyle);

        serializedObject.ApplyModifiedProperties();
    }
}