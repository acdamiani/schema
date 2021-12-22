using UnityEngine;
using UnityEditor;
using Schema.Runtime;
using System;
using System.Linq;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(BlackboardEntrySelector), true)]
public class BlackboardEntrySelectorDrawer : PropertyDrawer
{
    private Blackboard blackboard;
    private List<Type> filters;
    private string[] optionsList;
    private bool invalid = true;
    [SerializeField] private int index;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty idProperty = property.FindPropertyRelative("entryID");

        EditorGUI.BeginProperty(position, label, property);

        if (!blackboard)
        {
            blackboard = GetBlackboardInstance(property);
        }

        filters = GetTypeFilters(property);

        optionsList = GetOptionsList(filters, blackboard);

        if (invalid)
        {
            EditorGUI.Popup(position, label.text, 0, new string[] { "No valid keys found" });
        }
        else
        {
            List<BlackboardEntry> validEntries = blackboard.entries
                .FindAll(entry => filters.Contains(Type.GetType(entry.type)));

            foreach (BlackboardEntry entry in blackboard.entries)
            {
                if (entry.uID.Equals(idProperty.stringValue) && optionsList.ToList().Contains(entry.Name))
                    index = optionsList.ToList().IndexOf(entry.Name);
            }

            if (index >= optionsList.Length)
                index = 0;

            index = EditorGUI.Popup(
                position,
                label.text,
                index,
                optionsList
            );

            string selected = optionsList[index];

            int j = blackboard.entries.FindIndex(entry => entry.Name.Equals(selected));

            if (j == -1)
            {
                idProperty.stringValue = "";
            }
            else if (!blackboard.entries[j].uID.Equals(idProperty.stringValue))
            {
                idProperty.stringValue = blackboard.entries[j].uID;
            }
        }

        EditorGUI.EndProperty();
    }
    Blackboard GetBlackboardInstance(SerializedProperty property)
    {
        UnityEngine.Object targetObject = property.serializedObject.targetObject;

        if (targetObject is Node node)
        {
            return node.graph.blackboard;
        }
        else if (targetObject is Decorator decorator)
        {
            return decorator.node.graph.blackboard;
        }
        else
        {
            return null;
        }
    }
    List<Type> GetTypeFilters(SerializedProperty property)
    {
        SerializedProperty filters = property.FindPropertyRelative("filters");

        List<Type> types = new List<Type>();

        for (int i = 0; i < filters.arraySize; i++)
        {
            SerializedProperty filter = filters.GetArrayElementAtIndex(i);
            string str = filter.stringValue;

            types.Add(Type.GetType(str));
        }

        return types;
    }
    string[] GetOptionsList(List<Type> filters, Blackboard blackboard)
    {
        string[] arr = blackboard.entries.FindAll(entry => filters.Contains(Type.GetType(entry.type))).Select(entry => entry.Name).ToArray();
        invalid = arr.Length == 0;

        return arr;
    }
}