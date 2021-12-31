using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;

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
        Type type = Type.GetType(entry.type);

        string newName = EditorGUILayout.TextField("Name", entry.Name);
        string newType = possibleTypes[
            EditorGUILayout.Popup(
                "Type",
                possibleTypes.IndexOf(type),
                possibleTypes.Select(item => item.Name).ToArray()
                )
        ].AssemblyQualifiedName;

        if (!newName.Equals(entry.Name))
            entry.Name = newName;

        if (!newType.Equals(entry.type))
            entry.type = newType;

        EditorGUILayout.LabelField("Description");
        EditorStyles.textField.wordWrap = true;
        entry.description = EditorGUILayout.TextArea(entry.description);
    }
}