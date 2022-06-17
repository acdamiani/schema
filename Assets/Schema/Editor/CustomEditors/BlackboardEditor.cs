using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using Schema.Utilities;
using Schema;
using Schema.Internal;

namespace SchemaEditor.Editors
{
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
        private BlackboardEntry editing;
        private static Dictionary<Type, Tuple<string, Color>> entryGUIData = new Dictionary<Type, Tuple<string, Color>>();
        private bool isShowingGlobal;
        public void OnEnable()
        {
            if (target != null && target.GetType() == typeof(Blackboard))
                blackboard = (Blackboard)target;

            searchField = new SearchField();
        }
        public void DeselectAll()
        {
            GUI.FocusControl("");
            editing = null;
            selectedEntry = null;
            DestroyImmediate(entryEditor);
        }
        public override void OnInspectorGUI()
        {
            bool newIsShowingGlobal = GUILayout.Toolbar(isShowingGlobal ? 1 : 0, new string[] { "Local", "Global" }) == 1;

            if (isShowingGlobal != newIsShowingGlobal)
            {
                isShowingGlobal = newIsShowingGlobal;
                DeselectAll();
            }

            GUILayout.Space(8);

            GUILayout.BeginHorizontal(GUILayout.Height(16));

            GUILayout.Space(4);

            if (GUILayout.Button(Styles.plus, EditorStyles.iconButton, GUILayout.Width(16), GUILayout.ExpandHeight(true))) ShowContext();

            GUILayout.Space(10);

            searchValue = searchField.OnToolbarGUI(searchValue);

            GUILayout.Space(10);

            EditorGUI.BeginDisabledGroup(selectedEntry == null);
            if (GUILayout.Button(Styles.minus, EditorStyles.iconButton, GUILayout.Width(16), GUILayout.Height(16))) RemoveSelected();
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(4);

            GUILayout.EndHorizontal();

            serializedObject.Update();

            GUILayout.Space(10);

            BlackboardEntry[] globals = Blackboard.global.entries;
            BlackboardEntry[] locals = blackboard.entries;

            GUILayout.BeginVertical(Styles.blackboardScroll);

            GUILayout.Space(1);

            scroll = GUILayout.BeginScrollView(scroll, Styles.padding8x);

            if ((isShowingGlobal ? globals : locals).Length > 0)
                DrawEntryList(isShowingGlobal ? globals : locals);
            else
                GUILayout.Label(NodeEditor.NodeEditorPrefs.enableDebugViewPlus ? Styles.megamind : "No entries");

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && !clickedAny)
            {
                selectedEntry = null;
                GUI.FocusControl("");
                editing = null;
            }

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
                GUI.FocusControl("");

            GUILayout.EndScrollView();

            GUILayout.Space(1);

            GUILayout.EndVertical();

            GUILayout.Space(8);

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
                        editing = null;
                        GUI.FocusControl("");
                    }

                    selectedEntry = entry;
                    clickedAny = true;

                    Editor.CreateCachedEditor(selectedEntry, null, ref entryEditor);
                }
            }

            if (searchExcept.Count() == 0)
                GUILayout.Label(NodeEditor.NodeEditorPrefs.enableDebugViewPlus ? Styles.megamind : "No entries");
        }
        private void ShowContext()
        {
            GenericMenu menu = new GenericMenu();

            var keys = Blackboard.blackboardTypes;

            foreach (Type key in keys)
                menu.AddItem(
                    new GUIContent(NameAliases.GetAliasForType(key)),
                    false,
                    () => (isShowingGlobal ? Blackboard.global : blackboard).AddEntry(key)
                );

            menu.ShowAsContext();
        }
        private void RemoveSelected()
        {
            Blackboard current = isShowingGlobal ? Blackboard.global : blackboard;

            int i = Array.IndexOf(current.entries, selectedEntry) - 1;
            current.RemoveEntry(selectedEntry);
            i = i > 0 ? i : 0;

            if (current.entries.Length > 0)
                selectedEntry = current.entries[i];
            else
                selectedEntry = null;

            DestroyImmediate(entryEditor);
        }
        // private void Rename(BlackboardEntry entry, string name)
        // {
        //     if (String.IsNullOrEmpty(name))
        //         return;

        //     Undo.RegisterCompleteObjectUndo(entry, "Rename Entry");
        //     entry.name = name;
        // }
        private void DrawEntry(BlackboardEntry entry)
        {
            if (entry?.type == null)
                blackboard.RemoveEntry(entry, undo: false);

            Event current = Event.current;

            Vector2 nameSize = EditorStyles.whiteLabel.CalcSize(new GUIContent(entry.name));

            GUILayout.BeginVertical(selectedEntry == entry ? Styles.searchResult : GUIStyle.none, GUILayout.Height(32f));

            GUILayout.Space(8f);

            GUILayout.BeginHorizontal(GUILayout.Height(16f));

            GUILayout.Space(8f);

            GUILayout.Space(4f);

            GUIContent textContent = new GUIContent(entry == editing ? newEntryName : entry.name);

            Rect r = GUILayoutUtility.GetRect(textContent, entry == editing ? Styles.styles.nameField : Styles.styles.nodeText);
            Rect name = new Rect(r.x, r.y, r.width, 16f);

            if (current.clickCount == 2 && current.button == 0 && name.Contains(current.mousePosition))
            {
                editing = entry;
                GUI.FocusControl(entry.name);
                newEntryName = entry.name;
            }

            GUI.SetNextControlName(entry.name);

            if (entry == editing)
                newEntryName = GUI.TextField(name, textContent.text, Styles.styles.nameField);
            else
                GUI.Label(name, textContent, Styles.styles.nodeText);

            if (entry == editing && GUI.GetNameOfFocusedControl() != entry.name)
                editing = null;

            entryGUIData.TryGetValue(entry.type, out var entryData);

            if (entryData == null)
                entryData = entryGUIData[entry.type] = new Tuple<string, Color>(EntryType.GetName(entry.type), EntryType.GetColor(entry.type));

            GUILayout.FlexibleSpace();
            GUILayout.Label(entryData.Item1, EditorStyles.miniLabel);

            GUILayout.Space(4f);

            GUI.color = entryData.Item2;
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
}