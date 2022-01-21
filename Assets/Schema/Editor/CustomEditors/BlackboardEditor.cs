using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(Blackboard))]
public class BlackboardEditor : Editor
{
    private Blackboard blackboard;
    private Rect selectedRect;
    public int selectedIndex = -1;
    private string nameFieldControlName;
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

            GUI.color = Color.white;

            DrawEntry(blackboard.entries[i]);

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

                if (selectedIndex != i)
                    GUI.FocusControl("");
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
    private void DrawEntry(BlackboardEntry entry)
    {
        Event current = Event.current;

        Vector2 nameSize = EditorStyles.whiteLabel.CalcSize(new GUIContent(entry.Name));

        GUILayout.BeginVertical(GUILayout.Height(32f));
        GUILayout.Space(8f);
        GUILayout.BeginHorizontal(GUILayout.Height(16f));

        GUILayout.Space(8f);

        GUI.color = Blackboard.typeColors[entry.type];
        Rect imgRect = GUILayoutUtility.GetRect(new GUIContent(NodeEditorResources.circle), GUIStyle.none, GUILayout.Width(16), GUILayout.Height(16));
        GUI.DrawTexture(imgRect, NodeEditorResources.circle);
        GUI.color = Color.white;

        GUILayout.Space(8f);

        if (nameFieldControlName == entry.uID)
        {
            GUI.SetNextControlName(entry.uID);
            entry.Name = GUILayout.TextField(entry.Name, NodeEditorResources.styles.nameField);
        }
        else
        {
            GUILayout.Label(entry.Name, NodeEditorResources.styles.nameField);
        }

        Rect last = GUILayoutUtility.GetLastRect();
        Rect name = new Rect(last.x, last.y, nameSize.x, last.height);

        if (current.clickCount == 2 && current.button == 0 && name.Contains(current.mousePosition))
        {
            nameFieldControlName = entry.uID;
            GUI.FocusControl(entry.uID);
        }
        // nameFieldControlName is this entry but not editing text field
        else if (GUI.GetNameOfFocusedControl() != entry.uID && !String.IsNullOrEmpty(nameFieldControlName))
        {
            nameFieldControlName = "";
        }

        GUILayout.FlexibleSpace();
        GUILayout.Label(entry.type.Name, EditorStyles.miniLabel);
        GUILayout.Space(8f);

        Vector2 typeLabelSize = EditorStyles.miniLabel.CalcSize(new GUIContent(entry.type.Name));

        GUILayout.Space(8f);

        GUILayout.EndHorizontal();
        GUILayout.Space(8f);
        GUILayout.EndVertical();
    }
}