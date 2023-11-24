using System;
using System.Collections.Generic;
using System.Linq;
using Schema;
using Schema.Internal;
using Schema.Utilities;
using SchemaEditor;
using SchemaEditor.Internal;
using SchemaEditor.Internal.ComponentSystem;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Action = System.Action;

public class QuickSearch : IWindowComponentProvider
{
    private readonly CacheDictionary<Type, string> categories = new CacheDictionary<Type, string>();
    private readonly Action<Type> createNodeAction;
    private readonly List<string> favorites = Prefs.GetList("SCHEMA_PREF__favorites").ToList();
    private readonly CacheDictionary<Type, Texture2D> icons = new CacheDictionary<Type, Texture2D>();

    private readonly CacheDictionary<string, IEnumerable<Type>> search =
        new CacheDictionary<string, IEnumerable<Type>>();

    private readonly SearchField searchField;

    private readonly IEnumerable<Type> types;
    private readonly KeyCode[] validMovementCodes = { KeyCode.UpArrow, KeyCode.DownArrow };
    private bool close;
    private CacheDictionary<Type, string> descriptions = new CacheDictionary<Type, string>();
    private Vector2 mouseOverPosition;
    private Rect rect;
    private int refinementLength;
    private Vector2 scroll;

    private CacheDictionary<string, IEnumerable<Type>> searchedFavorites =
        new CacheDictionary<string, IEnumerable<Type>>();

    private bool searchFavorites;
    private string searchText = "";

    private int selected;
    private bool shouldFocus = true;
    private float toolbarHeight;

    public QuickSearch(IEnumerable<Type> types, Action<Type> onSelectAction)
    {
        this.types = types.Where(t => typeof(GraphObject).IsAssignableFrom(t));
        createNodeAction = onSelectAction;

        searchField = new SearchField();
        searchField.downOrUpArrowKeyPressed += UpOrDownArrowPressed;
    }

    public void HandleWinInfo(Rect rect, GUIContent title, GUIStyle style)
    {
        this.rect = rect;
    }

    public bool ShouldClose()
    {
        return close;
    }

    public void OnGUI(int id)
    {
        if (shouldFocus && Event.current.type == EventType.Layout)
        {
            GUI.FocusWindow(id);
            searchField.SetFocus();
            shouldFocus = false;
        }

        Focus();

        GUILayout.BeginHorizontal(Styles.SearchTopBar);

        Rect r = GUILayoutUtility.GetRect(GUIContent.none, Styles.SearchTopBarButton);
        r.y += Styles.SearchLarge.fixedHeight / 2f - r.height / 2f;

        searchFavorites = GUI.Toggle(r, searchFavorites, Icons.GetEditor("FolderFavorite Icon"),
            Styles.SearchTopBarButton);

        r = GUILayoutUtility.GetRect(new GUIContent(searchText), Styles.SearchLarge);

        if (Event.current.keyCode != KeyCode.Return)
            searchText = searchField.OnGUI(r, searchText, Styles.SearchLarge, Styles.CancelButton, GUIStyle.none);

        GUILayout.EndHorizontal();

        GUI.DrawTexture(new Rect(r.x + 16f, r.center.y, 16f, 16f).UseCenter(), Icons.GetEditor("Search Icon"));

        if (!string.IsNullOrEmpty(searchText))
            GUI.DrawTexture(new Rect(r.xMax - 16f, r.center.y, 16f, 16f).UseCenter(),
                Icons.GetResource("close", false));

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
            Styles.Padding8X,
            GUILayout.Width(rect.width),
            GUILayout.ExpandHeight(true)
        );

        DoResults();

        GUILayout.EndScrollView();

        GUI.DragWindow();
    }

    public void OnEnable()
    {
        shouldFocus = true;
    }

    public void OnDestroy()
    {
        // Unset SearchField focus
        GUIUtility.keyboardControl = 0;
        EditorGUIUtility.editingTextField = false;
    }

    private void DoSingleResult(Type type, string favoriteName, int index, Action onClick)
    {
        Event current = Event.current;

        float positionInView = index * 24f + Styles.Padding8X.padding.top;

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

        if (GUI.Toggle(r, isInFavorites, GUIContent.none, Styles.FavoriteToggle))
        {
            if (!isInFavorites)
            {
                favorites.Add(favoriteName);
                Prefs.SetList("SCHEMA_PREF__favorites", favorites);
            }
        }
        else
        {
            if (isInFavorites)
            {
                favorites.Remove(favoriteName);
                Prefs.SetList("SCHEMA_PREF__favorites", favorites);
            }
        }

        r = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.label, GUILayout.Height(24));
        bool insideRect = r.Contains(current.mousePosition);

        Vector2 delta = current.mousePosition - mouseOverPosition;

        switch (current.type)
        {
            case EventType.Repaint:
                if (index == realSelection)
                    SchemaGUI.DrawRoundedBox(r, GUI.skin.settings.selectionColor, 8);
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

        if (!string.IsNullOrEmpty(category))
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
            if (nodeType == typeof(Root))
                continue;

            Texture2D icon = icons.GetOrCreate(nodeType, () => GraphObject.GetIcon(nodeType));
            GUIContent content = new GUIContent(nodeType.Name, icon);

            DoSingleResult(
                nodeType,
                nodeType.Name,
                i,
                () => createNodeAction(nodeType)
            );

            i++;
        }

        GUILayout.EndVertical();
    }

    private void MoveSelectionByEvent()
    {
        Event current = Event.current;

        if (current.type == EventType.KeyDown)
            UpOrDownArrowPressed();
        else if (current.type == EventType.ScrollWheel)
            Scrolled();
    }

    private void UpOrDownArrowPressed()
    {
        Event current = Event.current;

        int resultsLength = search.GetOrCreate(searchText, () => SearchThroughResults(types, searchText)).Count();

        if (current.keyCode == KeyCode.UpArrow)
            MoveSelection(true, resultsLength);
        else if (current.keyCode == KeyCode.DownArrow)
            MoveSelection(false, resultsLength);
    }

    private void Scrolled()
    {
        Event current = Event.current;

        int resultsLength = search.GetOrCreate(searchText, () => SearchThroughResults(types, searchText)).Count();

        if (current.delta.y != 0)
        {
            MoveSelection(current.delta.y < 0, resultsLength);
            current.Use();
        }
    }

    private void MoveSelection(bool isUp, int resultsCount)
    {
        selected = isUp ? selected - 1 : selected + 1;
    }

    private void Focus()
    {
        selected = CorrectSelection(selected);

        float positionInView = selected * 24f + Styles.Padding8X.padding.top;

        if (positionInView < scroll.y)
            scroll.y = positionInView;
        else if (positionInView + 24 - scroll.y > rect.height - toolbarHeight)
            scroll.y = positionInView - rect.height + toolbarHeight + 36;
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
        if (string.IsNullOrWhiteSpace(query))
            return types;

        query = query.ToLower();
        string[] queries = query.Split(' ').Where(s => s != "").ToArray();

        List<Type> ret = new List<Type>();

        foreach (Type ty in types)
        foreach (string q in queries)
        {
            string category = categories.GetOrCreate(ty, () => GraphObject.GetCategory(ty)) ?? "";
            string haystack = category.ToLower() + ty.Name.ToLower();

            int s = haystack.IndexOf(q, StringComparison.Ordinal);
            if (s != -1)
            {
                ret.Add(ty);
                break;
            }
        }

        return ret;
    }
}