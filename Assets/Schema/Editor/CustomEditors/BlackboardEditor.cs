using System;
using System.Collections.Generic;
using System.Linq;
using Schema;
using Schema.Internal;
using Schema.Utilities;
using SchemaEditor.Utilities;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace SchemaEditor.Editors
{
    public class BlackboardEditor : Editor
    {
        private static readonly Dictionary<Type, Tuple<string, Color>> entryGUIData =
            new Dictionary<Type, Tuple<string, Color>>();

        public BlackboardEntry selectedEntry;
        private Blackboard blackboard;
        private bool clickedAny;
        private BlackboardEntry editing;
        private Editor entryEditor;
        private bool isShowingGlobal;
        private string newEntryName = "";
        private Vector2 scroll;
        private SearchField searchField;
        private string searchValue = "";

        public void OnEnable()
        {
            if (target != null && target.GetType() == typeof(Blackboard))
                blackboard = (Blackboard)target;

            searchField = new SearchField();
        }

        public override void OnInspectorGUI()
        {
            bool newIsShowingGlobal = GUILayout.Toolbar(isShowingGlobal ? 1 : 0, new[] { "Local", "Global" }) == 1;

            if (isShowingGlobal != newIsShowingGlobal)
                isShowingGlobal = newIsShowingGlobal;

            GUILayout.Space(8);

            GUILayout.BeginHorizontal(GUILayout.Height(16));

            GUILayout.Space(4);

            if (GUILayout.Button(Icons.GetEditor("Toolbar Plus More"), EditorStyles.miniButton, GUILayout.Width(16),
                    GUILayout.ExpandHeight(true))) ShowContext();

            GUILayout.Space(10);

            searchValue = searchField.OnToolbarGUI(searchValue);

            GUILayout.Space(10);

            EditorGUI.BeginDisabledGroup(selectedEntry == null);
            if (GUILayout.Button(Icons.GetEditor("Toolbar Minus"), EditorStyles.miniButton, GUILayout.Width(16),
                    GUILayout.Height(16))) RemoveSelected();
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(4);

            GUILayout.EndHorizontal();

            serializedObject.Update();

            GUILayout.Space(10);

            BlackboardEntry[] globals = Blackboard.global.entries;
            BlackboardEntry[] locals = blackboard.entries;

            scroll = GUILayout.BeginScrollView(scroll, Styles.blackboardEditorBackground);

            if ((isShowingGlobal ? globals : locals).Length > 0)
                DrawEntryList(isShowingGlobal ? globals : locals);
            else
                GUILayout.Label("No entries");

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && !clickedAny)
            {
                selectedEntry = null;
                GUI.FocusControl("");
                editing = null;
            }

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
                GUI.FocusControl("");

            GUILayout.EndScrollView();

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

            if (string.IsNullOrEmpty(searchValue))
                searchExcept = entries;
            else
                searchExcept = GetResults(entries, searchValue);

            foreach (BlackboardEntry entry in searchExcept)
            {
                GUI.color = Color.white;

                DrawEntry(entry);

                Rect r = GUILayoutUtility.GetLastRect();

                if (r.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown &&
                    Event.current.button == 0)
                {
                    if (selectedEntry != entry)
                    {
                        editing = null;
                        GUI.FocusControl("");
                    }

                    selectedEntry = entry;
                    clickedAny = true;

                    DestroyImmediate(entryEditor);
                    entryEditor = CreateEditor(selectedEntry, null);
                }
            }

            if (searchExcept.Count() == 0)
                GUILayout.Label("No entries");
        }

        private void ShowContext()
        {
            GenericMenu menu = new GenericMenu();

            Type[] keys = Blackboard.blackboardTypes;

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
            // if (entry?.type == null)
            //     blackboard.RemoveEntry(entry, undo: false);

            Event current = Event.current;

            GUIContent content = new GUIContent(entry.name);

            Rect rect = GUILayoutUtility.GetRect(content, Styles.blackboardEntry);

            bool isSelected = selectedEntry == entry;
            bool isHovered = rect.Contains(current.mousePosition);

            if (current.clickCount == 2 && current.button == 0 && isHovered)
            {
                editing = entry;
                GUI.FocusControl(entry.name);
                newEntryName = entry.name;
            }

            GUI.SetNextControlName(entry.name);

            if (entry == editing)
            {
                entry.name = EditorGUI.TextField(rect, entry.name, Styles.blackboardEntry);

                if (GUI.GetNameOfFocusedControl() != entry.name)
                    editing = null;
            }
            else
            {
                Styles.blackboardEntry.DrawIfRepaint(rect, content, isHovered, false, isSelected, false);
            }

            entryGUIData.TryGetValue(entry.type, out Tuple<string, Color> entryData);

            if (entryData == null)
                entryData = entryGUIData[entry.type] =
                    new Tuple<string, Color>(EntryType.GetName(entry.type), EntryType.GetColor(entry.type));

            rect = new Rect(rect.xMax - 24f, rect.y + rect.height / 2f - 8f, 16f, 16f);

            Color c = GUI.color;
            GUI.color = entryData.Item2;
            GUI.DrawTexture(rect, Icons.GetResource("in_connection", false));
            GUI.color = c;

            Vector2 size = EditorStyles.miniLabel.CalcSize(new GUIContent(entryData.Item1));
            rect = new Rect(rect.x - size.x - 4f, rect.y + rect.height / 2f - size.y / 2f, size.x, size.y);
            GUI.Label(rect, entryData.Item1, EditorStyles.miniLabel);
        }

        private IEnumerable<BlackboardEntry> GetResults(IEnumerable<BlackboardEntry> options, string query)
        {
            options = options
                .Where(e => e != null)
                .Where(e => e.name.ToLower().Contains(query.ToLower()));

            return options.OrderBy(e => StringSimilarity(e.name, query));
        }

        private int StringSimilarity(string s, string t)
        {
            if (string.IsNullOrEmpty(s))
            {
                if (string.IsNullOrEmpty(t))
                    return 0;
                return t.Length;
            }

            if (string.IsNullOrEmpty(t)) return s.Length;

            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // initialize the top and right of the table to 0, 1, 2, ...
            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 1; j <= m; d[0, j] = j++) ;

            for (int i = 1; i <= n; i++)
                for (int j = 1; j <= m; j++)
                {
                    int cost = t[j - 1] == s[i - 1] ? 0 : 1;
                    int min1 = d[i - 1, j] + 1;
                    int min2 = d[i, j - 1] + 1;
                    int min3 = d[i - 1, j - 1] + cost;
                    d[i, j] = Mathf.Min(Mathf.Min(min1, min2), min3);
                }

            return d[n, m];
        }
    }
}