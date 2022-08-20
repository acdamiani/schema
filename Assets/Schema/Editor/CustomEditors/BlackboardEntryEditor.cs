using System;
using System.Collections.Generic;
using System.Linq;
using Schema;
using Schema.Internal;
using UnityEditor;

namespace SchemaEditor
{
    [CustomEditor(typeof(BlackboardEntry))]
    public class BlackboardEntryEditor : Editor
    {
        private readonly List<Type> possibleTypes = new List<Type>();
        private SerializedProperty description;
        private SerializedProperty entryName;
        private SerializedProperty typeString;

        private void OnEnable()
        {
            foreach (Type t in Blackboard.blackboardTypes)
                possibleTypes.Add(t);

            entryName = serializedObject.FindProperty("m_Name");
            description = serializedObject.FindProperty("m_description");
            typeString = serializedObject.FindProperty("m_typeString");
        }

        public override void OnInspectorGUI()
        {
            BlackboardEntry entry = (BlackboardEntry)target;
            Type type = entry.type;

            serializedObject.Update();

            EditorGUILayout.PropertyField(entryName);
            string newType = Blackboard.blackboardTypes[
                EditorGUILayout.Popup(
                    "Type",
                    Array.IndexOf(Blackboard.blackboardTypes, type),
                    Blackboard.blackboardTypes.Select(item => EntryType.GetName(item)).ToArray()
                )
            ].AssemblyQualifiedName;

            if (!newType.Equals(typeString.stringValue))
            {
                typeString.stringValue = newType;
                Blackboard.InvokeEntryTypeChanged(entry);
            }

            EditorGUILayout.PropertyField(description);

            serializedObject.ApplyModifiedProperties();
        }
    }
}