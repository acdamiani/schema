using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomPropertyDrawer(typeof(TagFilter))]
public class TagFilterDrawer : PropertyDrawer
{
    private bool initializedMask;
    private int lastMask;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty selectedTags = property.FindPropertyRelative("tags");

        if (!initializedMask)
        {
            //Initialize mask from serialized string array
            List<string> values = new List<string>();

            for (int k = 0; k < selectedTags.arraySize; k++)
                values.Add(selectedTags.GetArrayElementAtIndex(k).stringValue);

            //To fill int completely if we get all the tags
            int tagCount = 0;
            for (int i = 0; i < InternalEditorUtility.tags.Length; i++)
            {
                string tag = InternalEditorUtility.tags[i];

                if (values.Contains(tag))
                {
                    lastMask = lastMask | (1 << i);
                    tagCount++;
                }
            }

            if (tagCount == InternalEditorUtility.tags.Length)
                lastMask = -1;

            initializedMask = true;
        }

        EditorGUI.BeginProperty(position, label, property);

        int nextMask = EditorGUI.MaskField(position, label, lastMask, InternalEditorUtility.tags);

        if (lastMask != nextMask)
        {
            selectedTags.ClearArray();

            int tagCount = 0;

            for (int i = 0; i < InternalEditorUtility.tags.Length; i++)
            {
                string tag = InternalEditorUtility.tags[i];

                if ((nextMask & (1 << i)) == 1 << i)
                {
                    selectedTags.InsertArrayElementAtIndex(tagCount);
                    selectedTags.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    selectedTags.GetArrayElementAtIndex(tagCount).stringValue = tag;

                    tagCount++;
                }
            }
        }

        lastMask = nextMask;

        EditorGUI.EndProperty();
    }
}