using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using Schema.Utilities;
using Schema.Editor;
public class BlackboardEditor : Editor
{
    private Blackboard blackboard;
    private Vector2 scroll;
    public BlackboardEntry selectedEntry;
    private bool clickedAny = false;
    private string newEntryName = "";
    private SearchField searchField;
    private string searchValue = "";
    private Editor entryEditor;
    private string editing;
    public void OnEnable()
    {
        if (target != null && target.GetType() == typeof(Blackboard))
            blackboard = (Blackboard)target;

        searchField = new SearchField();
    }
    public void DeselectAll()
    {
        if (!String.IsNullOrEmpty(editing))
            Rename(selectedEntry, newEntryName);
        GUI.FocusControl("");
        editing = "";
        selectedEntry = null;
    }
    public override void OnInspectorGUI()
    {
        GUILayout.BeginHorizontal();

        if (GUILayout.Button(Styles.plus, GUIStyle.none, GUILayout.Width(16), GUILayout.Height(16))) ShowContext();

        GUILayout.Space(10);

        searchValue = searchField.OnToolbarGUI(searchValue);

        GUILayout.Space(10);

        EditorGUI.BeginDisabledGroup(selectedEntry == null);
        if (GUILayout.Button(Styles.minus, GUIStyle.none, GUILayout.Width(16), GUILayout.Height(16))) RemoveSelected();
        EditorGUI.EndDisabledGroup();

        GUILayout.EndHorizontal();

        serializedObject.Update();

        GUILayout.Space(10);

        scroll = GUILayout.BeginScrollView(scroll);

        IEnumerable<BlackboardEntry> globals = blackboard.entries.FindAll(entry => entry.entryType == BlackboardEntry.EntryType.Global);
        IEnumerable<BlackboardEntry> locals = blackboard.entries.Except(globals);

        DrawEntryList(locals);

        EditorGUILayout.LabelField("Global Variables", EditorStyles.boldLabel);
        DrawEntryList(globals);

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && !clickedAny)
        {
            if (!String.IsNullOrEmpty(editing))
                Rename(selectedEntry, newEntryName);
            selectedEntry = null;
            GUI.FocusControl("");
            editing = "";
        }

        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            GUI.FocusControl("");

        GUILayout.EndScrollView();

        GUILayout.FlexibleSpace();

        if (entryEditor != null && entryEditor.target)
        {
            GUILayout.Label("Blackboard Entry", EditorStyles.boldLabel);
            entryEditor.OnInspectorGUI();
        }

        GUILayout.Space(10f);

        clickedAny = false;

        serializedObject.ApplyModifiedProperties();
    }
    private void DrawEntryList(IEnumerable<BlackboardEntry> entries)
    {
        IEnumerable<BlackboardEntry> searchExcept;

        if (String.IsNullOrEmpty(searchValue))
            searchExcept = entries;
        else
            searchExcept = GetResults(entries, searchValue);

        foreach (BlackboardEntry entry in searchExcept)
        {
            GUI.color = Color.white;

            DrawEntry(entry);

            Rect r = GUILayoutUtility.GetLastRect();

            if (r.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                if (selectedEntry != entry)
                {
                    if (!String.IsNullOrEmpty(editing))
                        Rename(selectedEntry, newEntryName);
                    editing = "";
                    GUI.FocusControl("");
                }

                selectedEntry = entry;
                clickedAny = true;

                Editor.CreateCachedEditor(selectedEntry, null, ref entryEditor);
            }
        }
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
        int i = blackboard.entries.IndexOf(selectedEntry) - 1;
        blackboard.RemoveEntry(selectedEntry);
        i = i > 0 ? i : 0;

        if (blackboard.entries.Count > 0)
            selectedEntry = blackboard.entries[i];
        else
            selectedEntry = null;
    }
    private void Rename(BlackboardEntry entry, string name)
    {
        if (String.IsNullOrEmpty(name))
            return;

        Undo.RegisterCompleteObjectUndo(entry, "Rename Entry");
        entry.name = name;
        BlackboardEntrySelectorDrawer.names[entry.uID] = entry.name;
    }
    private void DrawEntry(BlackboardEntry entry)
    {
        Event current = Event.current;

        Vector2 nameSize = EditorStyles.whiteLabel.CalcSize(new GUIContent(entry.name));

        if (selectedEntry == entry)
        {
            GUI.color = GUI.skin.settings.selectionColor;
            GUILayout.BeginVertical(Styles.styles.nodeWithoutPadding, GUILayout.Height(32f));
            GUI.color = Color.white;
        }
        else
        {
            GUILayout.BeginVertical(GUILayout.Height(32f));
        }

        GUILayout.Space(8f);

        GUILayout.BeginHorizontal(GUILayout.Height(16f));

        GUILayout.Space(8f);

        Rect r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(16f), GUILayout.Width(16f));

        switch (entry.entryType)
        {
            case BlackboardEntry.EntryType.Local:
                GUI.Label(r, new GUIContent(Styles.local, "Local Variable"), GUIStyle.none);
                break;
            case BlackboardEntry.EntryType.Global:
                GUI.Label(r, new GUIContent(Styles.global, "Global Variable"), GUIStyle.none);
                break;
            case BlackboardEntry.EntryType.Shared:
                GUI.Label(r, new GUIContent(Styles.shared, "Shared Variable"), GUIStyle.none);
                break;
        }

        GUILayout.Space(4f);

        Rect name;

        GUIContent textContent = new GUIContent(entry.uID == editing ? newEntryName : entry.name);

        r = GUILayoutUtility.GetRect(textContent, entry.uID == editing ? Styles.styles.nameField : Styles.styles.nodeText);
        name = new Rect(r.x, r.y, r.width, 16f);

        if (current.clickCount == 2 && current.button == 0 && name.Contains(current.mousePosition))
        {
            editing = entry.uID;
            GUI.FocusControl(entry.uID);
            newEntryName = entry.name;
        }

        GUI.SetNextControlName(entry.uID);

        if (entry.uID == editing)
            newEntryName = GUI.TextField(name, textContent.text, Styles.styles.nameField);
        else
            GUI.Label(name, textContent, Styles.styles.nodeText);

        if (entry.uID == editing && GUI.GetNameOfFocusedControl() != entry.uID)
        {
            Rename(entry, newEntryName);
            Debug.Log("done");
            editing = "";
        }

        GUILayout.FlexibleSpace();
        GUILayout.Label(NameAliases.GetAliasForType(entry.type), EditorStyles.miniLabel);

        GUILayout.Space(4f);

        GUI.color = Blackboard.typeColors[entry.type];
        Rect imgRect = GUILayoutUtility.GetRect(new GUIContent(Styles.circle), GUIStyle.none, GUILayout.Width(16), GUILayout.Height(16));
        GUI.DrawTexture(imgRect, Styles.circle);
        GUI.color = Color.white;

        GUILayout.Space(8f);

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }
    IEnumerable<BlackboardEntry> GetResults(IEnumerable<BlackboardEntry> options, string query)
    {
        options = options
            .Where(e => e != null)
            .Where(e => e.name.ToLower().Contains(query.ToLower()));

        return options.OrderBy(e => StringSimilarity(e.name, query));
    }
    int StringSimilarity(string s, string t)
    {
        if (string.IsNullOrEmpty(s))
        {
            if (string.IsNullOrEmpty(t))
                return 0;
            return t.Length;
        }

        if (string.IsNullOrEmpty(t))
        {
            return s.Length;
        }

        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];

        // initialize the top and right of the table to 0, 1, 2, ...
        for (int i = 0; i <= n; d[i, 0] = i++) ;
        for (int j = 1; j <= m; d[0, j] = j++) ;

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                int min1 = d[i - 1, j] + 1;
                int min2 = d[i, j - 1] + 1;
                int min3 = d[i - 1, j - 1] + cost;
                d[i, j] = Mathf.Min(Mathf.Min(min1, min2), min3);
            }
        }
        return d[n, m];
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
