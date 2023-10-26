using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomPropertyDrawer(typeof(NavMeshAreaMask))]
public class NavMeshAreaMaskDrawer : PropertyDrawer
{
    private bool initializedMask;
    private int lastMask;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty selectedTags = property.FindPropertyRelative("areas");
        SerializedProperty mask = property.FindPropertyRelative("mask");

        if (!initializedMask && mask.intValue != -1)
        {
            //Initialize mask from serialized string array
            List<string> values = new List<string>();

            for (int k = 0; k < selectedTags.arraySize; k++)
                values.Add(selectedTags.GetArrayElementAtIndex(k).stringValue);

            //To fill int completely if we get all the tags
            int tagCount = values.Count;

            if (tagCount == InternalEditorUtility.tags.Length)
                lastMask = -1;
            else
                for (int i = 0; i < InternalEditorUtility.tags.Length; i++)
                {
                    string tag = InternalEditorUtility.tags[i];

                    if (values.Contains(tag)) lastMask = lastMask | (1 << i);
                }
        }
        else if (!initializedMask)
        {
            lastMask = mask.intValue;
        }

        initializedMask = true;

        EditorGUI.BeginProperty(position, label, property);

        int nextMask = EditorGUI.MaskField(position, label, lastMask, GameObjectUtility.GetNavMeshAreaNames());

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
        mask.intValue = lastMask;

        EditorGUI.EndProperty();
    }
}