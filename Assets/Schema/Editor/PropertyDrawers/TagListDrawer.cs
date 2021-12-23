using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomPropertyDrawer(typeof(TagList))]
public class TagListDrawer : PropertyDrawer
{
    int i;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty tag = property.FindPropertyRelative("tag");

        EditorGUI.BeginProperty(position, label, property);

        i = EditorGUI.Popup(position, label, i, UnityEditorInternal.InternalEditorUtility.tags.Select(item => new GUIContent(item)).ToArray());
        tag.stringValue = UnityEditorInternal.InternalEditorUtility.tags[i];

        EditorGUI.EndProperty();
    }
}