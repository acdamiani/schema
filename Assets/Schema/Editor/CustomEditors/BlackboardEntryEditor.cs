using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using Schema.Utilities;

[CustomEditor(typeof(BlackboardEntry))]
public class BlackboardEntryEditor : Editor
{
    private List<Type> possibleTypes = new List<Type>();
    SerializedProperty description;
    SerializedProperty typeString;
    SerializedProperty entryTypeEnum;
    private void OnEnable()
    {
        foreach (Type t in Blackboard.typeColors.Keys)
            possibleTypes.Add(t);

        description = serializedObject.FindProperty("m_description");
        typeString = serializedObject.FindProperty("m_typeString");
        entryTypeEnum = serializedObject.FindProperty("m_entryType");
    }
    public override void OnInspectorGUI()
    {
        BlackboardEntry entry = (BlackboardEntry)target;
        Type type = entry.type;

        serializedObject.Update();

        string newName = EditorGUILayout.TextField("Name", entry.name);
        string newType = possibleTypes[
            EditorGUILayout.Popup(
                "Type",
                possibleTypes.IndexOf(type),
                possibleTypes.Select(item => NameAliases.GetAliasForType(item)).ToArray()
                )
        ].AssemblyQualifiedName;

        EditorGUILayout.PropertyField(entryTypeEnum);

        if (!newName.Equals(entry.name))
            entry.name = newName;

        if (!newType.Equals(entry.typeString))
        {
            entry.type = Type.GetType(newType);
            Blackboard.InvokeEntryTypeChanged(entry);
        }

        EditorGUILayout.PropertyField(description);

        serializedObject.ApplyModifiedProperties();
    }
}