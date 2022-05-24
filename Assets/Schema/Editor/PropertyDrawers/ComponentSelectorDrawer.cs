using UnityEngine;
using UnityEditor;
using System;
using Schema.Utilities;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(Schema.Internal.ComponentSelectorBase), true)]
public class ComponentSelectorDrawer : PropertyDrawer
{
    private static Dictionary<string, Type> fieldTypes = new Dictionary<string, Type>();
    private static Dictionary<string, float> scrolls = new Dictionary<string, float>();
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
        {
            fieldTypes[property.propertyPath] = Type.GetType(fieldValueType.stringValue);
        }

        if (!scrolls.ContainsKey(property.propertyPath))
            scrolls[property.propertyPath] = 0f;

        Rect r = EditorGUI.PrefixLabel(position, label);

        Vector2 size = EditorStyles.miniButtonRight.CalcSize(new GUIContent(Styles.menu));

        Rect buttonRect = new Rect(r.x + r.width - size.x, r.y, size.x, EditorGUIUtility.singleLineHeight);
        Rect useSelfLabel = new Rect(r.x, r.y, 49f, EditorGUIUtility.singleLineHeight);
        Rect useSelfRect = new Rect(r.x + 54f, r.y, 72f, EditorGUIUtility.singleLineHeight);
        Rect textRect = new Rect(r.x + 72f, r.y + 3f, r.width - 72f - size.x, EditorGUIUtility.singleLineHeight);

        GUIContent c = EditorGUIUtility.ObjectContent(null, fieldTypes[property.propertyPath]);

        GUI.Label(useSelfLabel, "Use Self", EditorStyles.label);
        EditorGUI.PropertyField(useSelfRect, useSelf, GUIContent.none);

        if (!useSelf.boolValue)
        {
            // EditorGUI.BeginDisabledGroup(entry.objectReferenceValue == null);
            // EditorGUI.EndDisabledGroup();
            BlackboardEntrySelectorDrawer.DoSelectorMenu(buttonRect, property, fieldInfo);
        }

        string name = entry.objectReferenceValue == null ? "null" : entry.objectReferenceValue.name;

        string s = "{0} " + fieldTypes[property.propertyPath].Name + " on " + (useSelf.boolValue ? "self" : name);

        Vector2 oldIconSize = EditorGUIUtility.GetIconSize();
        EditorGUIUtility.SetIconSize(Vector2.one * 12f);
        size = IconText.GetSize(s, EditorStyles.miniLabel, c.image);

        GUI.BeginClip(textRect, new Vector2(scrolls[property.propertyPath], 0f), Vector2.zero, false);
        IconText.DoIconText(new Rect(0f, 0f, textRect.width, EditorGUIUtility.singleLineHeight), s, EditorStyles.miniLabel, c.image);
        GUI.EndClip();

        if (size.x > textRect.width && textRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.ScrollWheel)
        {
            scrolls[property.propertyPath] = Mathf.Clamp(scrolls[property.propertyPath] - Event.current.delta.y * 10, -(size.x - textRect.width), 0f);
            // Prevent scroll
            Event.current.delta = Vector2.zero;
        }
        EditorGUIUtility.SetIconSize(oldIconSize);
    }
}