using UnityEngine;
using UnityEditor;
using System;
using Schema.Utilities;
using Schema.Editor;
public class BlackboardEditor : Editor
{
    private Blackboard blackboard;
    private Rect selectedRect;
    public BlackboardEntry selectedEntry;
    private string nameFieldControlName;
    public void OnEnable()
    {
        if (target != null && target.GetType() == typeof(Blackboard))
            blackboard = (Blackboard)target;
    }
    public override void OnInspectorGUI()
    {
        bool clickedAny = false;

        GUILayout.BeginHorizontal();

        if (GUILayout.Button(NodeEditorResources.plus, GUIStyle.none, GUILayout.Width(16), GUILayout.Height(16))) ShowContext();

        GUILayout.FlexibleSpace();

        EditorGUI.BeginDisabledGroup(selectedEntry == null);
        if (GUILayout.Button(NodeEditorResources.minus, GUIStyle.none, GUILayout.Width(16), GUILayout.Height(16))) RemoveSelected();
        EditorGUI.EndDisabledGroup();

        GUILayout.EndHorizontal();

        serializedObject.Update();

        GUILayout.Space(10);

        for (int i = 0; i < blackboard.entries.Count; i++)
        {
            BlackboardEntry entry = blackboard.entries[i];

            GUI.color = GUI.skin.settings.selectionColor;
            if (selectedEntry == entry)
                GUI.Box(selectedRect, "", NodeEditorResources.styles.node);

            GUI.color = Color.white;

            DrawEntry(entry);

            Rect r = GUILayoutUtility.GetLastRect();

            if (selectedEntry == entry && Event.current.type == EventType.Repaint)
            {
                selectedRect = r;
            }

            if (r.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                if (selectedEntry != entry)
                    GUI.FocusControl("");

                selectedEntry = entry;
                selectedRect = r;
                clickedAny = true;
            }
        }

        EditorGUILayout.LabelField("Global Variables", EditorStyles.boldLabel);

        foreach (BlackboardEntry e in NodeEditor.globalBlackboard.entries)
        {
            DrawEntry(e);
        }

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && !clickedAny)
            selectedEntry = null;

        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            GUI.FocusControl("");

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
        blackboard.RemoveEntry(selectedEntry, false);
        int i = blackboard.entries.IndexOf(selectedEntry) - 1;
        i = i > 0 ? i : 0;

        if (blackboard.entries.Count > 0)
            selectedEntry = blackboard.entries[i];
        else
            selectedEntry = null;
    }
    private void DrawEntry(BlackboardEntry entry)
    {
        Event current = Event.current;

        Vector2 nameSize = EditorStyles.whiteLabel.CalcSize(new GUIContent(entry.Name));

        GUILayout.BeginVertical(GUILayout.Height(32f));
        GUILayout.Space(8f);
        GUILayout.BeginHorizontal(GUILayout.Height(16f));

        GUILayout.Space(8f);

        Rect r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(16f), GUILayout.Width(16f));

        switch (entry.entryType)
        {
            case BlackboardEntry.EntryType.Local:
                GUI.Label(r, new GUIContent(NodeEditorResources.local, "Local Variable"), GUIStyle.none);
                break;
            case BlackboardEntry.EntryType.Global:
                GUI.Label(r, new GUIContent(NodeEditorResources.global, "Global Variable"), GUIStyle.none);
                break;
            case BlackboardEntry.EntryType.Shared:
                GUI.Label(r, new GUIContent(NodeEditorResources.shared, "Shared Variable"), GUIStyle.none);
                break;
        }

        GUILayout.Space(4f);

        if (nameFieldControlName == entry.uID)
        {
            GUI.SetNextControlName(entry.uID);
            entry.Name = GUILayout.TextField(entry.Name, NodeEditorResources.styles.nameField);
        }
        else
        {
            GUILayout.Label(new GUIContent(entry.Name, entry.description), NodeEditorResources.styles.nameField);
        }

        Rect last = GUILayoutUtility.GetLastRect();
        Rect name = new Rect(last.x, last.y, nameSize.x, last.height);

        if (current.clickCount == 2 && current.button == 0 && name.Contains(current.mousePosition))
        {
            nameFieldControlName = entry.uID;
            GUI.FocusControl(entry.uID);
        }
        // nameFieldControlName is this entry but not editing text field
        else if (GUI.GetNameOfFocusedControl() != entry.uID && nameFieldControlName == entry.uID)
        {
            nameFieldControlName = "";
        }

        GUILayout.FlexibleSpace();
        GUILayout.Label(NameAliases.GetAliasForType(entry.type), EditorStyles.miniLabel);

        GUILayout.Space(4f);

        GUI.color = Blackboard.typeColors[entry.type];
        Rect imgRect = GUILayoutUtility.GetRect(new GUIContent(NodeEditorResources.circle), GUIStyle.none, GUILayout.Width(16), GUILayout.Height(16));
        GUI.DrawTexture(imgRect, NodeEditorResources.circle);
        GUI.color = Color.white;

        GUILayout.Space(8f);

        GUILayout.EndHorizontal();
        GUILayout.Space(8f);
        GUILayout.EndVertical();
    }
}

[CustomEditor(typeof(Blackboard))]
public class BlackboardInspectorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginDisabledGroup(true);
        base.OnInspectorGUI();
        EditorGUI.EndDisabledGroup();
    }
}
