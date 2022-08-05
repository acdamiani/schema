using UnityEngine;
using Schema.Internal;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System;
using System.Linq;
using System.Collections.Generic;
using SchemaEditor.Internal.ComponentSystem;
using Schema.Utilities;

public class QuickSearch : IWindowComponentProvider
{
    private readonly KeyCode[] validMovementCodes = new KeyCode[] { KeyCode.UpArrow, KeyCode.DownArrow };
    private Rect rect;
    private SearchField searchField;
    private string searchText = "";
    private int refinementLength;
    private CacheDictionary<Type, string> categories = new CacheDictionary<Type, string>();
    private CacheDictionary<Type, string> descriptions = new CacheDictionary<Type, string>();
    private CacheDictionary<string, IEnumerable<Type>> search = new CacheDictionary<string, IEnumerable<Type>>();
    private CacheDictionary<string, IEnumerable<Type>> searchedFavorites = new CacheDictionary<string, IEnumerable<Type>>();
    private CacheDictionary<Type, Texture2D> icons = new CacheDictionary<Type, Texture2D>();
    private List<string> favorites = SchemaEditor.NodeEditor.Prefs.GetList("SCHEMA_PREF__favorites").ToList();
    private int selected = -1;
    private bool searchFavorites;
    private float keydownTime;
    private Vector2 scroll;
    private Vector2 mouseOverPosition;
    private float toolbarHeight;
    private bool didAddNode;
    private Action<Type> createNodeAction;
    private IEnumerable<Type> types;
    private bool close;
    public QuickSearch(IEnumerable<Type> types, Action<Type> onSelectAction)
    {
        this.types = types.Where(t => typeof(GraphObject).IsAssignableFrom(t));
        this.createNodeAction = onSelectAction;
    }
    public void HandleWinInfo(Rect rect, GUIContent title, GUIStyle style)
    {
        this.rect = rect;
    }
    public bool Close()
    {
        return close;
    }
    public void OnGUI(int id)
    {
        if (searchField == null)
        {
            searchField = new SearchField();
            searchField.downOrUpArrowKeyPressed += MoveSelectionByEvent;
        }

        searchField.SetFocus();

        GUILayout.BeginHorizontal(Styles.searchTopBar);

        Rect r = GUILayoutUtility.GetRect(GUIContent.none, Styles.searchTopBarButton);
        r.y += Styles.searchLarge.fixedHeight / 2f - r.height / 2f;

        searchFavorites = GUI.Toggle(r, searchFavorites, Styles.Icons.GetEditor("FolderFavorite Icon"), Styles.searchTopBarButton);

        GUI.SetNextControlName("searchField");

        r = GUILayoutUtility.GetRect(new GUIContent(searchText), Styles.searchLarge);

        if (Event.current.keyCode != KeyCode.Return)
            searchText = searchField.OnGUI(r, searchText, Styles.searchLarge, Styles.cancelButton, GUIStyle.none);

        GUI.DrawTexture(new Rect(r.x + 16f, r.center.y, 16f, 16f).UseCenter(), Styles.Icons.GetEditor("Search Icon"));

        if (!String.IsNullOrEmpty(searchText))
            GUI.DrawTexture(new Rect(r.xMax - 16f, r.center.y, 16f, 16f).UseCenter(), Styles.Icons.GetResource("close", false));

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
            GUILayout.Width(rect.width),
            GUILayout.ExpandHeight(true)
        );

        DoResults();

        GUILayout.EndScrollView();

        GUI.DragWindow();
    }
    private void DoSingleResult(Type type, string favoriteName, int index, Action onClick)
    {
        Event current = Event.current;

        float positionInView = index * 24f + Styles.padding8x.padding.top;

        if (positionInView + 24 < scroll.y || positionInView > rect.height - toolbarHeight + scroll.y)
        {
            GUILayoutUtility.GetRect(0f, 24f);
            return;
        }

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
                SchemaEditor.NodeEditor.Prefs.SetList("SCHEMA_PREF__favorites", favorites);
            }
        }
        else
        {
            if (isInFavorites)
            {
                favorites.Remove(favoriteName);
                SchemaEditor.NodeEditor.Prefs.SetList("SCHEMA_PREF__favorites", favorites);
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
                close = true;
                break;
        }

        GUILayout.EndHorizontal();
    }
    private void DoCompleteLabel(Rect rect, Type type)
    {
        rect = rect.Pad(new RectOffset(4, 4, 0, 0));

        Vector2 iconSize = EditorGUIUtility.GetIconSize();
        EditorGUIUtility.SetIconSize(new Vector2(16, 16));

        string category = categories.GetOrCreate(type, () => GraphObject.GetCategory(type));

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

        content = new GUIContent(type.Name, icons.GetOrCreate(type, () => GraphObject.GetIcon(type)));

        rect.x += rect.width;
        rect.width = EditorStyles.label.CalcSize(content).x;

        if (Event.current.type == EventType.Repaint)
            EditorStyles.label.Draw(rect, content, false, false, false, false);

        EditorGUIUtility.SetIconSize(iconSize);
    }
    private void DoResults()
    {
        GUILayout.BeginVertical();

        MoveSelectionByEvent();

        IEnumerable<Type> results = search.GetOrCreate(searchText, () => SearchThroughResults(types, searchText));

        if (searchFavorites)
            results = results.Where(x => favorites.Contains(x.Name));

        int i = 0;

        foreach (Type nodeType in results)
        {
            Texture2D icon = icons.GetOrCreate(nodeType, () => Schema.Internal.GraphObject.GetIcon(nodeType));
            GUIContent content = new GUIContent(nodeType.Name, icon);

            DoSingleResult(
                nodeType,
                nodeType.Name,
                i,
                () =>
                {
                    createNodeAction(nodeType);
                    didAddNode = true;
                }
            );

            i++;
        }

        GUILayout.EndVertical();
    }
    private void MoveSelectionByEvent()
    {
        Event current = Event.current;

        int resultsLength = search.GetOrCreate(searchText, () => SearchThroughResults(types, searchText)).Count();

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
    private void MoveSelection(bool isUp, int resultsCount)
    {
        selected = isUp ? selected - 1 : selected + 1;
        selected = CorrectSelection(selected);

        float positionInView = selected * 24f + Styles.padding8x.padding.top;

        if (positionInView < scroll.y)
            scroll.y = positionInView;
        else if (positionInView + 24 - scroll.y > rect.height - toolbarHeight)
            scroll.y = positionInView - rect.height + toolbarHeight + 24 + 8;
    }
    private int CorrectSelection(int selected)
    {
        int resultsLength = search.GetOrCreate(searchText, () => SearchThroughResults(types, searchText)).Count();

        selected = selected < 0 ? 0 : selected;
        selected = selected > resultsLength - 1 ? resultsLength - 1 : selected;

        return selected;
    }
    private IEnumerable<Type> SearchThroughResults(IEnumerable<Type> types, string query)
    {
        if (String.IsNullOrWhiteSpace(query))
            return types;

        query = query.ToLower();
        string[] queries = query.Split(' ').Where(s => s != "").ToArray();

        List<Type> ret = new List<Type>();

        foreach (Type ty in types)
        {
            foreach (string q in queries)
            {
                string category = categories.GetOrCreate(ty, () => GraphObject.GetCategory(ty)) ?? "";
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