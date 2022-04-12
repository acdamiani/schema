using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using Schema.Editor;

[CustomEditor(typeof(BlackboardEntry))]
public class BlackboardEntryEditor : Editor
{
    private List<Type> possibleTypes = new List<Type>();
    private void OnEnable()
    {
        foreach (Type t in Blackboard.typeColors.Keys)
        {
            possibleTypes.Add(t);
        }
    }
    public override void OnInspectorGUI()
    {
        BlackboardEntry entry = (BlackboardEntry)target;
        Type type = entry.type;

        string newName = EditorGUILayout.TextField("Name", entry.name);
        string newType = possibleTypes[
            EditorGUILayout.Popup(
                "Type",
                possibleTypes.IndexOf(type),
                possibleTypes.Select(item => item.Name).ToArray()
                )
        ].AssemblyQualifiedName;

        entry.entryType = (BlackboardEntry.EntryType)EditorGUILayout.EnumPopup("Variable Type", entry.entryType);

        if (!newName.Equals(entry.name))
        {
            entry.name = newName;
            BlackboardEntrySelectorDrawer.names[entry.uID] = newName;
        }

        if (!newType.Equals(entry.typeString))
        {
            entry.typeString = newType;
            Blackboard.InvokeEntryTypeChanged(entry);
        }

        EditorGUILayout.LabelField("Description");
        EditorStyles.textField.wordWrap = true;
        entry.description = EditorGUILayout.TextArea(entry.description);
    }
}