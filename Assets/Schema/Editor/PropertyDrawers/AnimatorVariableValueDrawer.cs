using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(AnimatorVariableValue))]
public class AnimatorVariableValueDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		SerializedProperty e = property.FindPropertyRelative("variableType");
		SerializedProperty s = property.FindPropertyRelative("variableName");

		SerializedProperty boolValue = property.FindPropertyRelative("boolValue");
		SerializedProperty intValue = property.FindPropertyRelative("intValue");
		SerializedProperty floatValue = property.FindPropertyRelative("floatValue");

		EditorGUI.BeginProperty(position, label, property);

		e.enumValueIndex = EditorGUILayout.Popup("Variable Type", e.enumValueIndex, e.enumDisplayNames);

		EditorGUILayout.BeginHorizontal();
		s.stringValue = EditorGUILayout.TextField(s.stringValue, GUILayout.ExpandWidth(true));
		GUILayout.FlexibleSpace();

		switch (e.enumValueIndex)
		{
			case 0:
				//Integer
				intValue.intValue = EditorGUILayout.IntField(intValue.intValue, GUILayout.MaxWidth(50f));
				break;
			case 1:
				//Float
				floatValue.floatValue = EditorGUILayout.FloatField(floatValue.floatValue, GUILayout.MaxWidth(50f));
				break;
			case 2:
				//Bool
				boolValue.boolValue = EditorGUILayout.Toggle(boolValue.boolValue, GUILayout.MaxWidth(50f));
				break;
		}

		EditorGUILayout.EndHorizontal();

		EditorGUI.EndProperty();
	}
}