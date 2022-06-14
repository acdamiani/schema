using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System;
using System.Linq;
using System.Collections.Generic;
using Schema.Utilities;

public static class QuickSearch
{
    private static readonly RectOffset windowPadding = new RectOffset(100, 100, 250, 100);
    private static readonly float keydownTimeWindow = 0.05f;
    private static readonly KeyCode[] validMovementCodes = new KeyCode[] { KeyCode.UpArrow, KeyCode.DownArrow };
    private static Rect searchRect;
    private static Schema.Graph target;
    private static Vector2 newNodePosition;
    private static SearchField searchField;
    private static string searchText = "";
    private static int refinementLength;
    private static CacheDictionary<Type, string> categories = new CacheDictionary<Type, string>();
    private static CacheDictionary<Type, string> descriptions = new CacheDictionary<Type, string>();
    private static CacheDictionary<string, IEnumerable<Type>> search = new CacheDictionary<string, IEnumerable<Type>>();
    private static CacheDictionary<Type, Texture2D> icons = new CacheDictionary<Type, Texture2D>();
    private static IEnumerable<Type> nodeTypes = HelperMethods.GetEnumerableOfType(typeof(Schema.Node));
    private static List<string> favorites = SchemaEditor.NodeEditor.NodeEditorPrefs.GetList("SCHEMA_PREF__favorites").ToList();
    private static int selected = -1;
    private static bool searchFavorites;
    private static float keydownTime;
    private static Vector2 scroll;
    private static bool focusSearch;
    private static Vector2 mouseOverPosition;
    private static float toolbarHeight;
    private static bool didAddNode;
    public static bool DoWindow(Rect window, Schema.Graph target, Vector2 newNodePosition, float timeSinceStartup)
    {
        didAddNode = false;

        QuickSearch.searchRect = new Rect(
            window.x + windowPadding.left,
            window.y + windowPadding.top,
            window.width - windowPadding.left - windowPadding.right,
            window.height - windowPadding.top - windowPadding.bottom
        );
        QuickSearch.target = target;
        QuickSearch.newNodePosition = newNodePosition;

        GUILayout.Window(1, searchRect, OnGUI, "", Styles.quickSearch);
        GUI.FocusWindow(1);

        return didAddNode;
    }
    public static void OnGUI(int id)
    {
        if (searchField == null)
        {
            searchField = new SearchField();
            searchField.downOrUpArrowKeyPressed += MoveSelectionByEvent;
        }

        searchField.SetFocus();

        GUILayout.BeginHorizontal(EditorStyles.toolbar);

        GUILayout.Space(8);

        GUI.SetNextControlName("searchField");

        if (Event.current.keyCode != KeyCode.Return)
            searchText = searchField.OnToolbarGUI(searchText);
        searchFavorites = GUILayout.Toggle(searchFavorites, "Favorites", EditorStyles.toolbarButton, GUILayout.Width(125));

        GUILayout.EndHorizontal();

        if (Event.current.type == EventType.Repaint)
            toolbarHeight = GUILayoutUtility.GetLastRect().height;
        else if (Event.current.type == EventType.KeyDown && validMovementCodes.Contains(Event.current.keyCode))
            MoveSelection(Event.current.keyCode == KeyCode.UpArrow, refinementLength);

        GUILayout.BeginScrollView(
            scroll,
            false,
            false,
            GUIStyle.none,
            GUIStyle.none,
            Styles.padding8x,
            GUILayout.Width(searchRect.width),
            GUILayout.ExpandHeight(true)
        );

        DoResults();

        GUILayout.EndScrollView();
    }
    private static void DoSingleResult(Type type, string favoriteName, int index, Action onClick)
    {
        Event current = Event.current;

        int realSelection = CorrectSelection(selected);

        GUILayout.BeginHorizontal(GUILayout.Height(24));

        Rect r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Width(24), GUILayout.Height(24));
        r = r.Pad(new RectOffset(4, 4, 4, 4));

        bool isInFavorites = favorites.Contains(favoriteName);

        if (GUI.Toggle(r, isInFavorites, GUIContent.none, Styles.favoriteToggle))
        {
            if (!isInFavorites)
            {
                favorites.Add(favoriteName);
                SchemaEditor.NodeEditor.NodeEditorPrefs.SetList("SCHEMA_PREF__favorites", favorites);
            }
        }
        else
        {
            if (isInFavorites)
            {
                favorites.Remove(favoriteName);
                SchemaEditor.NodeEditor.NodeEditorPrefs.SetList("SCHEMA_PREF__favorites", favorites);
            }
        }

        r = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.label, GUILayout.Height(24));
        bool insideRect = r.Contains(current.mousePosition);

        Vector2 delta = current.mousePosition - mouseOverPosition;

        switch (current.type)
        {
            case EventType.Repaint:
                Styles.searchResult.Draw(r, GUIContent.none, false, false, false, index == realSelection);
                DoCompleteLabel(r, type);
                break;
            case EventType.MouseMove:
                if (insideRect)
                    selected = index;
                break;
            case EventType.KeyDown when current.keyCode == KeyCode.Return && index == realSelection:
            case EventType.MouseDown when insideRect:
                onClick.Invoke();
                break;
        }

        GUILayout.EndHorizontal();
    }
    private static void DoCompleteLabel(Rect rect, Type type)
    {
        rect = rect.Pad(new RectOffset(4, 4, 0, 0));

        Vector2 iconSize = EditorGUIUtility.GetIconSize();
        EditorGUIUtility.SetIconSize(new Vector2(16, 16));

        string category = Schema.Node.GetNodeCategory(type) ?? "";

        if (!String.IsNullOrEmpty(category))
        {
            category = category.Trim('/')
                .Replace("/", " \u25B8 ");
            category += " \u25B8";
        }

        GUIContent content = new GUIContent(category);

        rect.width = EditorStyles.label.CalcSize(content).x;

        if (Event.current.type == EventType.Repaint)
            EditorStyles.label.Draw(rect, content, false, false, false, false);

        content = new GUIContent(type.Name, icons.GetOrCreate(type, () => Schema.Node.GetNodeIcon(type)));

        rect.x += rect.width;
        rect.width = EditorStyles.label.CalcSize(content).x;

        if (Event.current.type == EventType.Repaint)
            EditorStyles.label.Draw(rect, content, false, false, false, false);

        EditorGUIUtility.SetIconSize(iconSize);
    }
    private static void DoResults()
    {
        GUILayout.BeginVertical();

        MoveSelectionByEvent();

        IEnumerable<Type> results = search.GetOrCreate(searchText, () => SearchThroughResults(nodeTypes, searchText));

        int i = 0;

        foreach (Type nodeType in results)
        {
            Texture2D icon = icons.GetOrCreate(nodeType, () => Schema.Node.GetNodeIcon(nodeType));
            GUIContent content = new GUIContent(nodeType.Name, icon);

            DoSingleResult(
                nodeType,
                nodeType.Name,
                i,
                () =>
                {
                    target.AddNode(nodeType, newNodePosition);
                    didAddNode = true;
                }
            );

            i++;
        }

        GUILayout.EndVertical();
    }
    private static void MoveSelectionByEvent()
    {
        Event current = Event.current;

        int resultsLength = search.GetOrCreate(searchText, () => SearchThroughResults(nodeTypes, searchText)).Count();

        if (current.type == EventType.KeyDown)
        {
            if (current.keyCode == KeyCode.UpArrow)
                MoveSelection(true, resultsLength);
            else if (current.keyCode == KeyCode.DownArrow)
                MoveSelection(false, resultsLength);
        }
        else if (current.type == EventType.ScrollWheel && current.delta.y != 0)
        {
            MoveSelection(current.delta.y < 0, resultsLength);
        }
    }
    private static void MoveSelection(bool isUp, int resultsCount)
    {
        selected = isUp ? selected - 1 : selected + 1;
        selected = CorrectSelection(selected);

        float positionInView = selected * 24 + 8;

        if (positionInView - scroll.y < 0)
            scroll.y = positionInView;
        else if (positionInView + 24 - scroll.y > searchRect.height - toolbarHeight)
            scroll.y = positionInView - searchRect.height + toolbarHeight + 24 + 8;
    }
    private static int CorrectSelection(int selected)
    {
        int resultsLength = search.GetOrCreate(searchText, () => SearchThroughResults(nodeTypes, searchText)).Count();

        selected = selected < 0 ? 0 : selected;
        selected = selected > resultsLength - 1 ? resultsLength - 1 : selected;

        return selected;
    }
    private static IEnumerable<Type> SearchThroughResults(IEnumerable<Type> types, string query)
    {
        if (String.IsNullOrWhiteSpace(query))
        {
            return types;
        }

        query = query.ToLower();
        string[] queries = query.Split(' ').Where(s => s != "").ToArray();

        List<Type> ret = new List<Type>();

        foreach (Type ty in types)
        {
            foreach (string q in queries)
            {
                string category = categories.GetOrCreate(ty, () => Schema.Node.GetNodeCategory(ty)) ?? "";
                string search = category.ToLower() + ty.Name.ToLower();

                int s = Search(q, search);
                if (s != -1)
                {
                    ret.Add(ty);
                    break;
                }
            }
        }

        return ret;
    }
    private static int Search(string needle, string haystack)
    {
        int[] T = Preprocess(needle);
        int skip = 0;

        while (haystack.Length - skip >= needle.Length)
        {
            if (Same(haystack.Substring(skip), needle, needle.Length))
                return skip;
            skip = skip + T[haystack[skip + needle.Length - 1]];
        }

        return -1;
    }
    private static bool Same(string s1, string s2, int len)
    {
        int i = len - 1;
        while (s1[i] == s2[i])
        {
            if (i == 0)
                return true;
            i--;
        }
        return false;
    }
    private static int[] Preprocess(string pattern)
    {
        int[] T = new int[256];

        for (int i = 0; i < 256; i++)
            T[i] = pattern.Length;

        for (int i = 0; i < pattern.Length - 1; i++)
            T[pattern[i]] = pattern.Length - i - 1;

        return T;
    }
}