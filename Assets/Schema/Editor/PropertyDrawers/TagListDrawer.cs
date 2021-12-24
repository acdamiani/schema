using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomPropertyDrawer(typeof(TagList))]
public class TagListDrawer : PropertyDrawer
{
    int i = -1;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty tag = property.FindPropertyRelative("tag");

        if (i == -1)
        {
            string tagValue = tag.stringValue;

            i = UnityEditorInternal.InternalEditorUtility.tags.ToList().FindIndex(x => tagValue == x);

            i = i == -1 ? 0 : i;
        }

        EditorGUI.BeginProperty(position, label, property);

        i = EditorGUI.Popup(position, label, i, UnityEditorInternal.InternalEditorUtility.tags.Select(item => new GUIContent(item)).ToArray());
        tag.stringValue = UnityEditorInternal.InternalEditorUtility.tags[i];

        EditorGUI.EndProperty();
    }
}