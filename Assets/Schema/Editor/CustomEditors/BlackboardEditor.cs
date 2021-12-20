using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(Blackboard))]
public class BlackboardEditor : Editor
{
    private Blackboard blackboard;
    private Rect hoveredRect;
    private Rect selectedRect;
    public int selectedIndex = -1;
    public void OnEnable()
    {
        if (target != null && target.GetType() == typeof(Blackboard))
            blackboard = (Blackboard)target;
    }
    public override void OnInspectorGUI()
    {
        bool clickedAny = false;

        if (selectedIndex > blackboard.entries.Count - 1)
        {
            selectedIndex = blackboard.entries.Count - 1;
        }

        // if (!Schema.EditorInternal.NodeEditor.instance.windowInfo.searchIsShown)
        // {
        //     switch (Event.current.type)
        //     {
        //         case EventType.KeyDown:
        //             if (Event.current.keyCode == KeyCode.UpArrow)
        //             {
        //                 selectedIndex--;
        //                 selectedIndex = selectedIndex < 0 ? 0 : selectedIndex;
        //                 clickedAny = true;
        //             }
        //             else if (Event.current.keyCode == KeyCode.DownArrow)
        //             {
        //                 selectedIndex++;
        //                 selectedIndex = selectedIndex > blackboard.entries.Count - 1 ? blackboard.entries.Count - 1 : selectedIndex;
        //                 clickedAny = true;
        //             }
        //             break;
        //     }
        // }

        GUILayout.BeginHorizontal();

        if (GUILayout.Button(NodeEditorResources.plus, GUIStyle.none, GUILayout.Width(16), GUILayout.Height(16))) ShowContext();
        GUILayout.FlexibleSpace();

        EditorGUI.BeginDisabledGroup(selectedIndex == -1);
        if (GUILayout.Button(NodeEditorResources.minus, GUIStyle.none, GUILayout.Width(16), GUILayout.Height(16))) RemoveSelected();
        EditorGUI.EndDisabledGroup();

        GUILayout.EndHorizontal();

        serializedObject.Update();

        SerializedProperty entries = serializedObject.FindProperty("entries");

        GUILayout.Space(10);

        for (int i = 0; i < entries.arraySize; i++)
        {
            GUI.color = GUI.skin.settings.selectionColor;
            if (selectedIndex == i)
                GUI.Box(selectedRect, "", NodeEditorResources.styles.node);
            /* 			else if (hoveredIndex == i)
							EditorGUI.DrawRect(hoveredRect, Color.gray); */

            GUI.color = Color.white;

            EditorGUILayout.PropertyField(entries.GetArrayElementAtIndex(i), GUILayout.ExpandWidth(true), GUILayout.Height(32));

            Rect r = GUILayoutUtility.GetLastRect();

            if (selectedIndex == i && Event.current.type == EventType.Repaint)
            {
                selectedRect = r;
            }

            if (r.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                selectedIndex = i;
                selectedRect = r;
                clickedAny = true;
            }
        }

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && !clickedAny)
            selectedIndex = -1;

        serializedObject.ApplyModifiedProperties();
    }
    private void ShowContext()
    {
        GenericMenu menu = new GenericMenu();

        var keys = Blackboard.typeColors.Keys;

        foreach (Type key in keys)
        {
            menu.AddItem(new GUIContent(key.Name), false, () =>
            {
                blackboard.AddEntry(key);
            });
        }

        menu.ShowAsContext();
    }
    private void RemoveSelected()
    {
        blackboard.RemoveEntry(selectedIndex);
        selectedIndex--;
        selectedIndex = selectedIndex > 0 ? selectedIndex : 0;

        if (blackboard.entries.Count == 0) selectedIndex = -1;
    }
}