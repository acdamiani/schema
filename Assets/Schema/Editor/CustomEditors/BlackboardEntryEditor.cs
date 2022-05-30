using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using Schema.Internal;
using Schema.Utilities;

namespace SchemaEditor
{
    [CustomEditor(typeof(BlackboardEntry))]
    public class BlackboardEntryEditor : Editor
    {
        private List<Type> possibleTypes = new List<Type>();
        SerializedProperty description;
        SerializedProperty typeString;
        private void OnEnable()
        {
            foreach (Type t in Blackboard.blackboardTypes)
                possibleTypes.Add(t);

            description = serializedObject.FindProperty("m_description");
            typeString = serializedObject.FindProperty("m_typeString");
        }
        public override void OnInspectorGUI()
        {
            BlackboardEntry entry = (BlackboardEntry)target;
            Type type = entry.type;

            serializedObject.Update();

            string newName = EditorGUILayout.TextField("Name", entry.name);
            string newType = Blackboard.blackboardTypes[
                EditorGUILayout.Popup(
                    "Type",
                    Array.IndexOf(Blackboard.blackboardTypes, type),
                    Blackboard.blackboardTypes.Select(item => Schema.EntryType.GetName(item)).ToArray()
                    )
            ].AssemblyQualifiedName;

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
}