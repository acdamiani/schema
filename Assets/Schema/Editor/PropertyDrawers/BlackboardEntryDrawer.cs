using System;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(BlackboardEntry))]
public class BlackboardEntryDrawer : PropertyDrawer
{
	private Vector2 nameSize;
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);

		DrawBlackboard(position, property, label);

		EditorGUI.EndProperty();
	}
	private void DrawBlackboard(Rect position, SerializedProperty property, GUIContent label)
	{
		int oldIndentLevel = EditorGUI.indentLevel;

		SerializedObject obj = new SerializedObject(property.objectReferenceValue);
		SerializedProperty nameProp = obj.FindProperty("_name");
		SerializedProperty typeProp = obj.FindProperty("_type");

		nameSize = EditorStyles.whiteLargeLabel.CalcSize(new GUIContent(nameProp.stringValue));

		Rect imgRect = new Rect(position.x + position.width - 50f, position.y + position.height / 2f - 16f, 32f, 32f);
		Rect labelRect = new Rect(position.x + 15f, position.y + position.height / 2f - nameSize.y / 2f, nameSize.x, nameSize.y);

		GUI.Label(labelRect, nameProp.stringValue, EditorGUIUtility.isProSkin ? EditorStyles.whiteLargeLabel : EditorStyles.largeLabel);

		Type currentType = Type.GetType(typeProp.stringValue);
		string last = typeProp.stringValue;

		GUI.color = Blackboard.typeColors[currentType];

		Vector2 typeLabelSize = EditorStyles.miniLabel.CalcSize(new GUIContent(currentType.Name));

		GUI.DrawTexture(imgRect, NodeEditorResources.blackboardIcon);
		GUI.color = Color.white;
		GUI.Label(new Rect(imgRect.x - typeLabelSize.x - 5f, imgRect.y + imgRect.height / 2f - typeLabelSize.y / 2f, typeLabelSize.x, typeLabelSize.y), currentType.Name, EditorStyles.miniLabel);

		obj.ApplyModifiedProperties();

		EditorGUI.indentLevel = oldIndentLevel;
	}

	/* 	private void HandleEvents(Rect position, Event e)
		{
			switch (e.type)
			{
				case EventType.MouseDown:

					if (e.button == 0 && e.clickCount == 2 && position.Contains(e.mousePosition))
					{

					}
					break;
			}
		} */
}
