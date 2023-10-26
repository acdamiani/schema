using System;
using System.Collections.Generic;
using Schema.Internal;
using UnityEditor;
using UnityEngine;

namespace SchemaEditor
{
    [CustomPropertyDrawer(typeof(ComponentSelectorBase), true)]
    public class ComponentSelectorDrawer : PropertyDrawer
    {
        private static readonly Dictionary<string, Type> fieldTypes = new Dictionary<string, Type>();
        private static readonly Dictionary<string, float> scrolls = new Dictionary<string, float>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty fieldValueType = property.FindPropertyRelative("m_fieldValueType");

            if (fieldValueType == null)
            {
                Debug.LogWarning("Use ComponentSelector<T> instead of ComponentSelectorBase");
                return;
            }

            SerializedProperty useSelf = property.FindPropertyRelative("m_useSelf");
            SerializedProperty entry = property.FindPropertyRelative("m_entry");

            if (!fieldTypes.ContainsKey(property.propertyPath))
                fieldTypes[property.propertyPath] = Type.GetType(fieldValueType.stringValue);

            if (!scrolls.ContainsKey(property.propertyPath))
                scrolls[property.propertyPath] = 0f;

            Rect r = EditorGUI.PrefixLabel(position, label);

            Rect buttonRect = new Rect(r.x, r.y + EditorGUIUtility.singleLineHeight, r.width,
                EditorGUIUtility.singleLineHeight);
            Rect useSelfLabel = new Rect(r.x, r.y, 49f, EditorGUIUtility.singleLineHeight);
            Rect useSelfRect = new Rect(r.x + 54f, r.y, 72f, EditorGUIUtility.singleLineHeight);
            Rect textRect = new Rect(r.x + 70f, r.y + 3f, r.width - 70f, EditorGUIUtility.singleLineHeight);

            GUIContent c = EditorGUIUtility.ObjectContent(null, fieldTypes[property.propertyPath]);

            GUI.Label(useSelfLabel, "Use Self", EditorStyles.label);
            EditorGUI.PropertyField(useSelfRect, useSelf, GUIContent.none);

            string entryName = "";

            if (!useSelf.boolValue)
                entryName = BlackboardEntrySelectorDrawer.DoSelectorDrawer(buttonRect, property, GUIContent.none, null,
                    fieldInfo);
            else
                entryName = "self";

            string name = $"{{0}} {fieldTypes[property.propertyPath].Name} on {entryName}";

            Vector2 oldIconSize = EditorGUIUtility.GetIconSize();
            EditorGUIUtility.SetIconSize(Vector2.one * 12f);
            Vector2 size = SchemaGUI.GetSize(name, EditorStyles.miniLabel, c.image);

            GUI.BeginClip(textRect, new Vector2(scrolls[property.propertyPath], 0f), Vector2.zero, false);
            SchemaGUI.DoIconText(new Rect(0f, 0f, textRect.width, EditorGUIUtility.singleLineHeight), name,
                EditorStyles.miniLabel, c.image);
            GUI.EndClip();

            if (size.x > textRect.width && textRect.Contains(Event.current.mousePosition) &&
                Event.current.type == EventType.ScrollWheel)
            {
                scrolls[property.propertyPath] =
                    Mathf.Clamp(scrolls[property.propertyPath] - Event.current.delta.y * 10, -(size.x - textRect.width),
                        0f);
                // Prevent scroll
                Event.current.delta = Vector2.zero;
            }

            EditorGUIUtility.SetIconSize(oldIconSize);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty useSelf = property.FindPropertyRelative("m_useSelf");
            SerializedProperty entry = property.FindPropertyRelative("m_entry");

            if (useSelf.boolValue)
                return EditorGUIUtility.singleLineHeight;
            if (entry.objectReferenceValue != null)
                return EditorGUIUtility.singleLineHeight * 3f + 4f;
            return EditorGUIUtility.singleLineHeight * 2f;
        }
    }
}