using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomPropertyDrawer(typeof(TagList))]
public class TagListDrawer : PropertyDrawer
{
    private int i = -1;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty tag = property.FindPropertyRelative("tag");

        if (i == -1)
        {
            string tagValue = tag.stringValue;

            i = InternalEditorUtility.tags.ToList().FindIndex(x => tagValue == x);

            i = i == -1 ? 0 : i;
        }

        EditorGUI.BeginProperty(position, label, property);

        i = EditorGUI.Popup(position, label, i,
            InternalEditorUtility.tags.Select(item => new GUIContent(item)).ToArray());
        tag.stringValue = InternalEditorUtility.tags[i];

        EditorGUI.EndProperty();
    }
}