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

        GUILayout.BeginHorizontal();

        if (GUILayout.Button(NodeEditorResources.plus, GUIStyle.none, GUILayout.Width(16), GUILayout.Height(16))) ShowContext();
        GUILayout.FlexibleSpace();

        EditorGUI.BeginDisabledGroup(selectedIndex == -1);
        if (GUILayout.Button(NodeEditorResources.minus, GUIStyle.none, GUILayout.Width(16), GUILayout.Height(16))) RemoveSelected();
        EditorGUI.EndDisabledGroup();

        GUILayout.EndHorizontal();

        serializedObject.Update();

        GUILayout.Space(10);

        for (int i = 0; i < blackboard.entries.Count; i++)
        {
            GUI.color = GUI.skin.settings.selectionColor;
            if (selectedIndex == i)
                GUI.Box(selectedRect, "", NodeEditorResources.styles.node);
            /* 			else if (hoveredIndex == i)
							EditorGUI.DrawRect(hoveredRect, Color.gray); */

            GUI.color = Color.white;

            DrawEntry(blackboard.entries[i].Name, Type.GetType(blackboard.entries[i].typeString));

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
    private void DrawEntry(string name, Type type)
    {
        int oldIndentLevel = EditorGUI.indentLevel;

        Vector2 nameSize = EditorStyles.whiteLargeLabel.CalcSize(new GUIContent(name));

        GUILayout.BeginHorizontal(GUILayout.Height(32f));

        GUILayout.Space(8f);

        GUILayout.Label(name, EditorGUIUtility.isProSkin ? EditorStyles.whiteLargeLabel : EditorStyles.largeLabel);

        GUILayout.FlexibleSpace();
        GUILayout.Label(type.Name, EditorStyles.miniLabel, GUILayout.Height(32));
        GUILayout.Space(8f);

        GUI.color = Blackboard.typeColors[type];
        Vector2 typeLabelSize = EditorStyles.miniLabel.CalcSize(new GUIContent(type.Name));
        Rect imgRect = GUILayoutUtility.GetRect(new GUIContent(NodeEditorResources.blackboardIcon), GUIStyle.none, GUILayout.Width(32), GUILayout.Height(32));
        GUI.DrawTexture(imgRect, NodeEditorResources.blackboardIcon);
        GUI.color = Color.white;

        GUILayout.Space(8f);

        GUILayout.EndHorizontal();

        EditorGUI.indentLevel = oldIndentLevel;
    }
}