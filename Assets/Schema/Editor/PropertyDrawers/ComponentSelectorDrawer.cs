using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(Schema.Internal.ComponentSelector), true)]
public class ComponentSelectorDrawer : PropertyDrawer
{
    private static Dictionary<string, Type> fieldTypes = new Dictionary<string, Type>();
    private static Dictionary<string, float> scrolls = new Dictionary<string, float>();
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty fieldValue = property.FindPropertyRelative("fieldValue");

        if (fieldValue == null)
        {
            Debug.LogWarning("Use ComponentSelector<T> instead of ComponentSelector");
            return;
        }

        SerializedProperty fieldValueType = property.FindPropertyRelative("fieldValueType");
        SerializedProperty useSelf = property.FindPropertyRelative("useSelf");

        if (!fieldTypes.ContainsKey(property.propertyPath))
        {
            System.Type parentType = fieldValue.serializedObject.targetObject.GetType();

            System.Reflection.FieldInfo fi = parentType.GetField(
                fieldValue.propertyPath,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );

            Debug.Log(fi?.FieldType);
            fieldTypes[property.propertyPath] = parentType;
        }

        if (!scrolls.ContainsKey(property.propertyPath))
            scrolls[property.propertyPath] = 0f;

        Rect r = EditorGUI.PrefixLabel(position, label);

        float buttonSize = Mathf.Min(position.height, EditorGUIUtility.singleLineHeight);

        Rect buttonRect = new Rect(r.x + r.width - buttonSize, r.y, buttonSize, buttonSize);
        Rect fieldRect = new Rect(r.x, r.y, r.width - buttonSize, r.height);

        GUIContent c = EditorGUIUtility.ObjectContent(null, fieldTypes[property.propertyPath]);

        // EditorGUI.PropertyField(fieldRect, fieldValue, GUIContent.none);

        // EditorGUI.PropertyField(fieldRect, useSelf);

        Vector2 oldIconSize = EditorGUIUtility.GetIconSize();
        EditorGUIUtility.SetIconSize(Vector2.one * 12f);
        GUIContent l = new GUIContent($" Will use component {fieldTypes[property.propertyPath].Name} on agent", c.image);
        // BlackboardEntrySelectorDrawer.DoSelectorMenu(buttonRect, property, fieldInfo);
        Vector2 size = EditorStyles.miniLabel.CalcSize(l);
        GUI.BeginClip(fieldRect, new Vector2(scrolls[property.propertyPath], 0f), Vector2.zero, false);
        GUI.Label(new Rect(0f, 0f, size.x, 14f), l, EditorStyles.miniLabel);
        GUI.EndClip();

        if (size.x > fieldRect.width && fieldRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.ScrollWheel)
        {
            scrolls[property.propertyPath] = Mathf.Clamp(scrolls[property.propertyPath] - Event.current.delta.y * 10, -(size.x - fieldRect.width), 0f);
            // Prevent scroll
            Event.current.delta = Vector2.zero;
        }
        EditorGUIUtility.SetIconSize(oldIconSize);
    }
}