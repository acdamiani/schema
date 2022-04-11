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
        SerializedProperty fieldValue = property.FindPropertyRelative("fieldValue");

        if (fieldValue == null)
        {
            Debug.LogWarning("Use ComponentSelector<T> instead of ComponentSelectorBase");
            return;
        }

        SerializedProperty fieldValueType = property.FindPropertyRelative("fieldValueType");
        SerializedProperty useSelf = property.FindPropertyRelative("useSelf");
        SerializedProperty entryID = property.FindPropertyRelative("entryID");

        if (!fieldTypes.ContainsKey(property.propertyPath))
        {
            System.Type parentType = fieldValue.serializedObject.targetObject.GetType();
            FieldInfo fi = parentType.GetFieldFromPath(fieldValue.propertyPath);
            fieldTypes[property.propertyPath] = fi.FieldType;
        }

        if (!scrolls.ContainsKey(property.propertyPath))
            scrolls[property.propertyPath] = 0f;

        Rect r = EditorGUI.PrefixLabel(position, label);

        float buttonSize = Mathf.Min(position.height, EditorGUIUtility.singleLineHeight);

        Rect buttonRect = new Rect(r.x + r.width - buttonSize, r.y + EditorGUIUtility.singleLineHeight, buttonSize, buttonSize);
        Rect fieldRect = new Rect(r.x, r.y + EditorGUIUtility.singleLineHeight, r.width - buttonSize, EditorGUIUtility.singleLineHeight);
        Rect useSelfLabel = new Rect(r.x, r.y, 49f, EditorGUIUtility.singleLineHeight);
        Rect useSelfRect = new Rect(r.x + 54f, r.y, 72f, EditorGUIUtility.singleLineHeight);
        Rect textRect = new Rect(r.x + 72f, r.y + 3f, r.width - 72f, EditorGUIUtility.singleLineHeight);

        GUIContent c = EditorGUIUtility.ObjectContent(null, fieldTypes[property.propertyPath]);

        GUI.Label(useSelfLabel, "Use Self", EditorStyles.label);
        EditorGUI.PropertyField(useSelfRect, useSelf, GUIContent.none);

        if (!useSelf.boolValue)
        {
            EditorGUI.BeginDisabledGroup(!String.IsNullOrEmpty(entryID.stringValue));
            EditorGUI.PropertyField(fieldRect, fieldValue, GUIContent.none);
            EditorGUI.EndDisabledGroup();
            BlackboardEntrySelectorDrawer.DoSelectorMenu(buttonRect, property, fieldInfo);
        }

        string name = !String.IsNullOrEmpty(entryID.stringValue) ?
                        BlackboardEntrySelectorDrawer.GetNameForID(entryID.stringValue) :
                        fieldValue.objectReferenceValue?.name;

        name = String.IsNullOrEmpty(name) ? "null" : name;

        string s = "{0} " + fieldTypes[property.propertyPath].Name + " on " + (useSelf.boolValue ? "self" : name);

        Vector2 oldIconSize = EditorGUIUtility.GetIconSize();
        EditorGUIUtility.SetIconSize(Vector2.one * 12f);
        Vector2 size = IconText.GetSize(s, EditorStyles.miniLabel, c.image);

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
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty useSelf = property.FindPropertyRelative("useSelf");

        if (useSelf == null || useSelf.boolValue)
            return EditorGUIUtility.singleLineHeight;
        else
            return EditorGUIUtility.singleLineHeight * 2;
    }
}