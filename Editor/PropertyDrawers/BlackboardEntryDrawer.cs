// using System;
// using System.Reflection;
// using System.Linq;
// using UnityEngine;
// using UnityEditor;

// [CustomPropertyDrawer(typeof(BlackboardEntry))]
// public class BlackboardEntryDrawer : PropertyDrawer
// {
//     private Vector2 nameSize;
//     SerializedObject obj;
//     SerializedProperty nameProp;
//     SerializedProperty typeProp;
//     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//     {
//         EditorGUI.BeginProperty(position, label, property);

//         DrawBlackboard(position, property, label);

//         EditorGUI.EndProperty();
//     }
//     private void DrawBlackboard(Rect position, SerializedProperty property, GUIContent label)
//     {
//         int oldIndentLevel = EditorGUI.indentLevel;

//         if (obj == null)
//             obj = new SerializedObject(property.objectReferenceValue);

//         if (nameProp == null)
//             nameProp = obj.FindProperty("_name");

//         if (typeProp == null)
//             typeProp = obj.FindProperty("_type");

//         string entryName = "Test Name";

//         nameSize = EditorStyles.whiteLargeLabel.CalcSize(new GUIContent(entryName));

//         Rect imgRect = new Rect(position.x + position.width - 50f, position.y + position.height / 2f - 16f, 32f, 32f);
//         Rect labelRect = new Rect(position.x + 15f, position.y + position.height / 2f - nameSize.y / 2f, nameSize.x, nameSize.y);

//         GUI.Label(labelRect, entryName, EditorGUIUtility.isProSkin ? EditorStyles.whiteLargeLabel : EditorStyles.largeLabel);

//         //string last = typeProp.stringValue;
//         Type currentType = typeof(float);//Type.GetType(last);

//         GUI.color = Blackboard.typeColors[currentType];

//         Vector2 typeLabelSize = EditorStyles.miniLabel.CalcSize(new GUIContent(currentType.Name));

//         GUI.DrawTexture(imgRect, NodeEditorResources.blackboardIcon);
//         GUI.color = Color.white;
//         GUI.Label(new Rect(imgRect.x - typeLabelSize.x - 5f, imgRect.y + imgRect.height / 2f - typeLabelSize.y / 2f, typeLabelSize.x, typeLabelSize.y), currentType.Name, EditorStyles.miniLabel);

//         EditorGUI.indentLevel = oldIndentLevel;
//     }

//     /* 	private void HandleEvents(Rect position, Event e)
// 		{
// 			switch (e.type)
// 			{
// 				case EventType.MouseDown:

// 					if (e.button == 0 && e.clickCount == 2 && position.Contains(e.mousePosition))
// 					{

// 					}
// 					break;
// 			}
// 		} */
// }

