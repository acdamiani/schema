using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(NavMeshAreaMask))]
public class NavMeshAreaMaskDrawer : PropertyDrawer
{
    private bool initializedMask;
    private int lastMask;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty selectedTags = property.FindPropertyRelative("areas");
        SerializedProperty mask = property.FindPropertyRelative("mask");

        if (!initializedMask)
        {
            //Initialize mask from serialized string array
            List<string> values = new List<string>();

            for (int k = 0; k < selectedTags.arraySize; k++)
            {
                values.Add(selectedTags.GetArrayElementAtIndex(k).stringValue);
            }

            //To fill int completely if we get all the tags
            int tagCount = 0;
            for (int i = 0; i < UnityEditorInternal.InternalEditorUtility.tags.Length; i++)
            {
                string tag = UnityEditorInternal.InternalEditorUtility.tags[i];

                if (values.Contains(tag))
                {
                    lastMask = lastMask | (1 << i);
                    tagCount++;
                }
            }

            if (tagCount == UnityEditorInternal.InternalEditorUtility.tags.Length)
                lastMask = -1;

            initializedMask = true;
        }

        EditorGUI.BeginProperty(position, label, property);

        int nextMask = EditorGUI.MaskField(position, label, lastMask, GameObjectUtility.GetNavMeshAreaNames());

        if (lastMask != nextMask)
        {
            selectedTags.ClearArray();

            int tagCount = 0;

            for (int i = 0; i < UnityEditorInternal.InternalEditorUtility.tags.Length; i++)
            {
                string tag = UnityEditorInternal.InternalEditorUtility.tags[i];

                if ((nextMask & (1 << i)) == (1 << i))
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