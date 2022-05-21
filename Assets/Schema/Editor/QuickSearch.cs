using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System;
using System.Linq;
using System.Collections.Generic;

public static class QuickSearch
{
    private static readonly RectOffset windowPadding = new RectOffset(100, 100, 250, 100);
    private static readonly float keydownTimeWindow = 0.05f;
    private static Rect searchRect;
    private static Schema.Graph target;
    private static Vector2 newNodePosition;
    private static SearchField searchField;
    private static string searchText;
    private static bool folderView = true;
    private static Vector2 offset;
    private static bool isTransitioning;
    private static float transitionStartTime;
    private static Dictionary<string, IEnumerable<Type>> categories;
    private static Dictionary<Type, Texture2D> nodeIcons = new Dictionary<Type, Texture2D>();
    private static string currentCategory;
    private static string selectedCategory;
    private static List<string> favorites = SchemaEditor.NodeEditor.NodeEditorPrefs.GetList("SCHEMA_PREF__favorites").ToList();
    private static int selected = -1;
    private static bool searchFavorites;
    private static float keydownTime;
    private static Vector2 scrollA;
    private static Vector2 _scrollA;
    private static Vector2 scrollB;
    public static bool DoSearch(Rect window, Schema.Graph target, Vector2 newNodePosition, float timeSinceStartup)
    {
        if (searchField == null)
            searchField = new SearchField();

        if (categories == null)
            categories = Schema.Node.GetNodeCategories();

        QuickSearch.searchRect = new Rect(
            window.x + windowPadding.left,
            window.y + windowPadding.top,
            window.width - windowPadding.left - windowPadding.right,
            window.height - windowPadding.top - windowPadding.bottom
        );
        QuickSearch.target = target;
        QuickSearch.newNodePosition = newNodePosition;

        GUI.Box(searchRect, "", Styles.quickSearch);

        GUILayout.BeginArea(searchRect);

        GUILayout.BeginHorizontal(EditorStyles.toolbar);

        GUILayout.Space(8);

        EditorGUI.BeginChangeCheck();

        GUI.SetNextControlName("searchField");
        searchText = searchField.OnToolbarGUI(searchText);
        searchFavorites = GUILayout.Toggle(searchFavorites, "Favorites", EditorStyles.toolbarButton, GUILayout.Width(125));

        searchField.downOrUpArrowKeyPressed += () => GUI.FocusControl("");

        if (EditorGUI.EndChangeCheck())
            selected = -1;

        GUILayout.EndHorizontal();

        GUILayout.EndArea();

        float headerHeight = 24;

        bool didAddNode = false;

        if (isTransitioning && Event.current.type == EventType.Layout)
        {
            float t = !folderView ? -searchRect.width : 0f;

            float newX = Mathf.SmoothStep(offset.x, t, (((timeSinceStartup - transitionStartTime) % 1.5f) / 1.5f));

            if (Mathf.Abs(newX - t) < 1f)
            {
                isTransitioning = false;
                offset = new Vector2(0f, 0f);
            }
            else
            {
                offset = new Vector2(newX, 0f);
            }
        }

        Rect clip = new Rect(searchRect.x, searchRect.y + headerHeight, searchRect.width, searchRect.height - headerHeight);

        GUI.BeginClip(clip, offset, Vector2.zero, false);

        if (isTransitioning)
        {
            GUILayout.BeginHorizontal();

            GUILayout.BeginScrollView(scrollA, GUILayout.Width(searchRect.width), GUILayout.Height(searchRect.height - headerHeight));

            RenderFolderView();

            GUILayout.EndScrollView();

            GUILayout.BeginScrollView(scrollB, GUILayout.Width(searchRect.width), GUILayout.Height(searchRect.height - headerHeight));

            RenderCategoryView();

            GUILayout.EndScrollView();

            GUILayout.EndHorizontal();
        }
        else
        {
            if (folderView)
            {
                scrollA = GUILayout.BeginScrollView(scrollA, GUILayout.Width(searchRect.width), GUILayout.Height(searchRect.height - headerHeight));

                if (String.IsNullOrWhiteSpace(searchText))
                    didAddNode = RenderFolderView();
                else
                    didAddNode = RenderNodeResults(SearchThroughResults(categories.Select(x => x.Value).SelectMany(x => x), searchText), 0);

                GUILayout.EndScrollView();
            }
            else
            {
                scrollB = GUILayout.BeginScrollView(scrollB, GUILayout.Width(searchRect.width), GUILayout.Height(searchRect.height - headerHeight));

                if (String.IsNullOrWhiteSpace(searchText))
                    didAddNode = RenderCategoryView();
                else
                    didAddNode = RenderNodeResults(SearchThroughResults(categories.Select(x => x.Value).SelectMany(x => x), searchText), 0);

                GUILayout.EndScrollView();
            }
        }

        GUI.EndClip();

        return didAddNode;
    }
    private static bool RenderFolderView()
    {
        IEnumerable<string> categoryNames = categories.Keys.Where(x => !String.IsNullOrEmpty(x));

        GUILayout.BeginVertical();

        GUILayout.Label("Add Node", Styles.searchTitle, GUILayout.Height(32));

        bool didAddNode = false;

        if (String.IsNullOrWhiteSpace(searchText))
        {
            for (int i = 0; i < categoryNames.Count(); i++)
            {
                string s = categoryNames.ElementAt(i);

                if (s == "")
                    continue;

                GUIContent content = new GUIContent(s);

                Rect r = GUILayoutUtility.GetRect(content, Styles.searchResult, GUILayout.Height(24));
                r = new Rect(r.position + new Vector2(24, 0), r.size - new Vector2(24, 0));

                Vector2 b = EditorStyles.iconButton.CalcSize(new GUIContent(Styles.next));
                Rect buttonRect = new Rect(r.xMax - b.x - 5f, r.y + r.height / 2f - b.y / 2f, b.x, b.y);

                Color c = GUI.color;
                GUI.color = GUI.skin.settings.selectionColor;

                Vector2 pos = Event.current.mousePosition;

                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && r.Contains(pos) && !buttonRect.Contains(pos))
                {
                    GUI.FocusControl("");

                    if (Event.current.clickCount == 1)
                    {
                        selected = i;
                    }
                    else if (Event.current.clickCount == 2)
                    {
                        SwitchView();
                        selectedCategory = s;
                    }
                }

                string selectedFolder = categoryNames.ElementAtOrDefault(selected);

                if (selectedFolder == s)
                    GUI.Box(r, "", Styles.styles.node);

                r = new Rect(r.x + 5f, r.y, r.width - 5f, r.height);

                GUI.color = c;

                GUI.DrawTexture(new Rect(r.x - 25, r.y + r.height / 2f - 8, 16, 16), Styles.folder);

                GUI.Label(r, content, Styles.searchResult);

                if (GUI.Button(buttonRect, new GUIContent(Styles.next), EditorStyles.iconButton))
                {
                    SwitchView();
                    selectedCategory = s;
                }
            }
        }

        IEnumerable<Type> nodeTypes = searchFavorites ? categories[""].Where(x => favorites.Contains(x.FullName)) : categories[""];

        didAddNode = RenderNodeResults(SearchThroughResults(nodeTypes, searchText), String.IsNullOrWhiteSpace(searchText) ? categoryNames.Count() : 0);

        GUILayout.EndVertical();

        return didAddNode;
    }
    private static bool RenderCategoryView()
    {
        GUILayout.BeginVertical();

        Vector2 iconSize = EditorGUIUtility.GetIconSize();

        EditorGUIUtility.SetIconSize(new Vector2(36, 36));
        GUIContent t = new GUIContent(selectedCategory, Styles.folderOpen);
        Rect r = GUILayoutUtility.GetRect(t, Styles.searchTitle);
        r = new Rect(r.position + new Vector2(24, 0), r.size - new Vector2(24, 0));

        EditorGUIUtility.SetIconSize(new Vector2(16, 16));

        Vector2 b = EditorStyles.iconButton.CalcSize(new GUIContent(Styles.prev));

        if (GUI.Button(new Rect(r.x - 20, r.y + r.height / 2f - b.y / 2f, b.x, b.y), new GUIContent(Styles.prev), EditorStyles.iconButton))
            SwitchView();

        GUI.Label(r, t, Styles.searchTitle);

        IEnumerable<Type> nodeTypes = searchFavorites ? categories[selectedCategory].Where(x => favorites.Contains(x.FullName)) : categories[selectedCategory];

        bool didAddNode = RenderNodeResults(SearchThroughResults(nodeTypes, searchText), 0);

        EditorGUIUtility.SetIconSize(iconSize);

        GUILayout.EndVertical();

        return didAddNode;
    }
    private static void SwitchView()
    {
        folderView = !folderView;
        isTransitioning = true;
        transitionStartTime = (float)EditorApplication.timeSinceStartup;
        offset = new Vector2(folderView ? -searchRect.width : 0f, 0f);
        selected = -1;
    }
    // private static Tuple<bool, Type> RenderContents(IEnumerable<Type> contents, Type selected, string title, string searchValue)
    // {
    //     int searchLen = String.IsNullOrWhiteSpace(searchValue) ? 0 : searchValue.Length;

    //     Dictionary<Type, int> searched = contents.ToDictionary(x => x, x => 0);

    //     if (!String.IsNullOrWhiteSpace(searchValue))
    //         searched = (Dictionary<Type, int>)SearchThroughResults(contents, searchValue);

    //     GUILayout.BeginVertical(GUILayout.ExpandWidth(true));

    //     Vector2 iconSize = EditorGUIUtility.GetIconSize();

    //     EditorGUIUtility.SetIconSize(new Vector2(36, 36));

    //     GUIContent t = new GUIContent(title, Styles.folderOpen);
    //     Rect r = GUILayoutUtility.GetRect(t, Styles.searchTitle);
    //     r = new Rect(r.position + new Vector2(24, 0), r.size - new Vector2(24, 0));

    //     EditorGUIUtility.SetIconSize(new Vector2(16, 16));

    //     Vector2 b = EditorStyles.iconButton.CalcSize(new GUIContent(Styles.prev));

    //     bool goBack = GUI.Button(new Rect(r.x - 20, r.y + r.height / 2f - b.y / 2f, b.x, b.y), new GUIContent(Styles.prev), EditorStyles.iconButton);

    //     GUI.Label(r, t, Styles.searchTitle);

    //     Type newSelected = RenderNodeResults(searched.ToDictionary(x => x.Key, x => new Tuple<int, int>(x.Value, searchLen)), selected);

    //     EditorGUIUtility.SetIconSize(iconSize);

    //     GUILayout.Space(8);

    //     GUILayout.EndVertical();

    //     return new Tuple<bool, Type>(goBack, newSelected);
    // }
    private static bool RenderNodeResults(IDictionary<Type, Tuple<int, int>> results, int offset)
    {
        GUILayout.BeginVertical();

        Vector2 iconSize = EditorGUIUtility.GetIconSize();

        EditorGUIUtility.SetIconSize(new Vector2(16, 16));

        bool didAddNode = false;

        for (int i = 0; i < results.Count; i++)
        {
            KeyValuePair<Type, Tuple<int, int>> result = results.ElementAt(i);

            Type type = result.Key;

            bool isSelected = i == selected - offset;

            Texture2D icon;

            if (!nodeIcons.ContainsKey(type))
                icon = nodeIcons[type] = Schema.Node.GetNodeIcon(type);
            else
                icon = nodeIcons[type];

            GUIContent c = new GUIContent(type.Name, icon);

            Rect r = GUILayoutUtility.GetRect(c, Styles.searchResult, GUILayout.Height(24));
            r = new Rect(r.position + new Vector2(24, 0), r.size - new Vector2(24, 0));

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && r.Contains(Event.current.mousePosition))
            {
                GUI.FocusControl("");

                if (Event.current.clickCount == 1)
                {
                    selected = i + offset;
                }
                else if (Event.current.clickCount == 2)
                {
                    target.AddNode(type, newNodePosition);
                    didAddNode = true;
                }
            }
            else if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.UpArrow)
            {
                if (EditorApplication.timeSinceStartup - keydownTime >= keydownTimeWindow)
                {
                    keydownTime = (float)EditorApplication.timeSinceStartup;
                    selected = selected < 0 ? 0 : selected - 1;

                    if (folderView)
                    {
                        if (selected * 24 - scrollA.y < 0)
                            scrollA.y = selected * 24 + 16;
                        else if (selected * 24 + 48 - scrollA.y > searchRect.height - 48)
                            scrollA.y = selected * 24 - searchRect.height + 112;
                    }
                    else
                    {
                        if (selected * 24 - scrollB.y < 0)
                            scrollB.y = selected * 24 + 16;
                        else if (selected * 24 + 48 - scrollB.y > searchRect.height - 48)
                            scrollB.y = selected * 24 - searchRect.height + 112;
                    }

                    selected = Mathf.Clamp(selected, 0, offset + results.Count - 1);
                }
            }
            else if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.DownArrow)
            {
                if (EditorApplication.timeSinceStartup - keydownTime >= keydownTimeWindow)
                {
                    keydownTime = (float)EditorApplication.timeSinceStartup;
                    selected = selected < 0 ? 0 : selected + 1;

                    if (folderView)
                    {
                        if (selected * 24 + 48 - scrollA.y > searchRect.height - 48)
                            scrollA.y = selected * 24 - searchRect.height + 112;
                        else if (selected * 24 - scrollA.y < 0)
                            scrollA.y = selected * 24 + 16;
                    }
                    else
                    {
                        if (selected * 24 + 48 - scrollB.y > searchRect.height - 48)
                            scrollB.y = selected * 24 - searchRect.height + 112;
                        else if (selected * 24 - scrollB.y < 0)
                            scrollB.y = selected * 24 + 16;
                    }

                    selected = Mathf.Clamp(selected, 0, offset + results.Count - 1);
                }
            }
            else if (isSelected && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                target.AddNode(type, newNodePosition);

                didAddNode = true;
            }
            else if (Event.current.type == EventType.KeyUp)
            {
                keydownTime = 0f;
            }

            Color col = GUI.color;

            if (favorites.Contains(type.FullName))
                GUI.color = EditorGUIUtility.isProSkin ? new Color32(252, 191, 7, 255) : new Color32(199, 150, 0, 255);
            else
                GUI.color = Styles.windowAccent;

            if (GUI.Button(new Rect(r.x - 20, r.y + r.height / 2f - 8, 16, 16), Styles.favorite, GUIStyle.none))
            {
                if (favorites.Contains(type.FullName))
                    favorites.Remove(type.FullName);
                else
                    favorites.Add(type.FullName);

                SchemaEditor.NodeEditor.NodeEditorPrefs.SetList("SCHEMA_PREF__favorites", favorites);
            }

            GUI.color = GUI.skin.settings.selectionColor;

            if (isSelected)
                GUI.Box(r, "", Styles.styles.node);

            r = new Rect(r.x + 5, r.y, r.width - 5, r.height);

            GUI.color = col;

            DoHighlightedLabel(r, c, result.Value.Item1, result.Value.Item2, Styles.searchResult, Styles.searchHighlight);
        }

        EditorGUIUtility.SetIconSize(iconSize);

        GUILayout.EndVertical();

        return didAddNode;
    }
    private static void DoHighlightedLabel(Rect rect, GUIContent content, int index, int length, GUIStyle regularStyle, GUIStyle highlightedStyle)
    {
        index = Mathf.Clamp(index, 0, content.text.Length - 1);
        length = Mathf.Clamp(length, 0, content.text.Length - index);

        if (index == content.text.Length || length == 0)
        {
            GUI.Label(rect, content, regularStyle);
            return;
        }
        else if (index == 0 && length == content.text.Length)
        {
            GUI.Label(rect, content, highlightedStyle);

            return;
        }
        else if (index == 0)
        {
            GUIContent first = new GUIContent(content.text.Substring(index, length), content.image, content.tooltip);
            GUIContent second = new GUIContent(content.text.Substring(length, content.text.Length - length), content.tooltip);

            Vector2 s1 = highlightedStyle.CalcSize(first);
            Vector2 s2 = regularStyle.CalcSize(second);

            Rect r1 = new Rect(rect.x, rect.y, s1.x, rect.height);
            Rect r2 = new Rect(rect.x + s1.x, rect.y, s2.x, rect.height);

            GUI.Label(r1, first, highlightedStyle);
            GUI.Label(r2, second, regularStyle);
        }
        else if (index + length - 1 >= content.text.Length - 1)
        {
            GUIContent first = new GUIContent(content.text.Substring(0, index), content.image, content.tooltip);
            GUIContent second = new GUIContent(content.text.Substring(index, length), content.tooltip);

            Vector2 s1 = regularStyle.CalcSize(first);
            Vector2 s2 = highlightedStyle.CalcSize(second);

            Rect r1 = new Rect(rect.x, rect.y, s1.x, rect.height);
            Rect r2 = new Rect(rect.x + s1.x, rect.y, s2.x, rect.height);

            GUI.Label(r1, first, regularStyle);
            GUI.Label(r2, second, highlightedStyle);
        }
        else
        {
            GUIContent first = new GUIContent(content.text.Substring(0, index), content.image, content.tooltip);
            GUIContent second = new GUIContent(content.text.Substring(index, length), content.tooltip);
            GUIContent third = new GUIContent(content.text.Substring(index + length), content.tooltip);

            Vector2 s1 = regularStyle.CalcSize(first);
            Vector2 s2 = highlightedStyle.CalcSize(second);
            Vector2 s3 = regularStyle.CalcSize(third);

            Rect r1 = new Rect(rect.x, rect.y, s1.x, rect.height);
            Rect r2 = new Rect(rect.x + s1.x, rect.y, s2.x, rect.height);
            Rect r3 = new Rect(r2.x + s2.x, rect.y, s3.x, rect.height);

            GUI.Label(r1, first, regularStyle);
            GUI.Label(r2, second, highlightedStyle);
            GUI.Label(r3, third, regularStyle);
        }
    }
    private static IDictionary<Type, Tuple<int, int>> SearchThroughResults(IEnumerable<Type> types, string query)
    {
        if (String.IsNullOrWhiteSpace(query))
            return types.ToDictionary(x => x, x => new Tuple<int, int>(0, 0));

        query = query.ToLower();

        Dictionary<Type, Tuple<int, int>> d = new Dictionary<Type, Tuple<int, int>>();

        foreach (Type ty in types)
        {
            int s = Search(query, ty.Name.ToLower());
            if (s != -1)
                d[ty] = new Tuple<int, int>(s, query.Length);
        }

        return d;
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