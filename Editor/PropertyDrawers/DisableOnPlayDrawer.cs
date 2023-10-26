using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(DisableOnPlayAttribute))]
public class DisableOnPlayDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginDisabledGroup(Application.isPlaying);
        EditorGUI.PropertyField(position, property, label, true);
        EditorGUI.EndDisabledGroup();
    }
}