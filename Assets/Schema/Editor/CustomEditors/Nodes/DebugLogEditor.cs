using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DebugLog)), CanEditMultipleObjects]
public class DebugLogEditor : Editor
{
    SerializedProperty message;
    SerializedProperty keys;
    string m;
    void OnEnable()
    {
        message = serializedObject.FindProperty("message");
        keys = serializedObject.FindProperty("keys");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(message);
        EditorGUILayout.PropertyField(keys);

        EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);

        try
        {
            m = String.Format(message.stringValue, GetKeyNames((DebugLog)targets[0], keys));
        }
        catch (Exception e)
        {
            EditorGUILayout.HelpBox(e.Message, MessageType.Warning);
        }
        EditorGUILayout.HelpBox(m, MessageType.None);

        serializedObject.ApplyModifiedProperties();
    }
    private string[] GetKeyNames(DebugLog obj, SerializedProperty keys)
    {
        string[] arr = new string[keys.arraySize];

        for (int i = 0; i < keys.arraySize; i++)
        {
            SerializedProperty element = keys.GetArrayElementAtIndex(i);
            SerializedProperty entryID = element.FindPropertyRelative("entryID");

            if (String.IsNullOrEmpty(entryID.stringValue))
                continue;

            arr[i] = Blackboard.instance.GetEntry(entryID.stringValue).Name;
        }

        return arr;
    }
}