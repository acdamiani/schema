using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System;
using System.Linq;
using System.Collections.Generic;

public static class QuickSearch
{
    private static readonly RectOffset windowPadding = new RectOffset(100, 100, 250, 100);
    private static SearchField searchField;
    private static string searchText;
    private static bool folderOverview = true;
    private static Vector2 offset;
    private static bool isTransitioning;
    private static float transitionStartTime;
    private static Dictionary<string, IEnumerable<Type>> categories;
    private static Dictionary<Type, Texture2D> nodeIcons = new Dictionary<Type, Texture2D>();
    private static string selectedCategory;
    private static List<string> favorites = SchemaEditor.NodeEditor.NodeEditorPrefs.GetList("SCHEMA_PREF__favorites").ToList();
    public static void DoSearch(Rect window, float timeSinceStartup)
    {
        if (searchField == null)
            searchField = new SearchField();

        if (categories == null)
            categories = Schema.Node.GetNodeCategories();

        Rect searchRect = new Rect(
            window.x + windowPadding.left,
            window.y + windowPadding.top,
            window.width - windowPadding.left - windowPadding.right,
            window.height - windowPadding.top - windowPadding.bottom
        );

        GUILayout.BeginArea(searchRect);

        GUILayout.BeginVertical(Styles.quickSearch, GUILayout.ExpandHeight(true));

        searchText = searchField.OnGUI(searchText);

        GUIContent c = new GUIContent("jsoidfjoi", Styles.errorIcon);
        GUIContent[] test = new GUIContent[] { new GUIContent("ehll", Styles.global), new GUIContent("sdjfio", Styles.favorite), new GUIContent("h", Styles.local), c };

        Rect lastContentPos = GUILayoutUtility.GetRect(0f, 0f);
        Rect clip = new Rect(lastContentPos.x, lastContentPos.y, lastContentPos.width, searchRect.height - lastContentPos.y);

        if (isTransitioning && Event.current.type == EventType.Layout)
        {
            float target = !folderOverview ? -searchRect.width : 0f;

            float newX = Mathf.SmoothStep(offset.x, target, (((timeSinceStartup - transitionStartTime) % 1.5f) / 1.5f));

            if (Mathf.Abs(newX - target) < 1f)
            {
                isTransitioning = false;
                offset = new Vector2(0f, 0f);
            }
            else
            {
                offset = new Vector2(newX, 0f);
            }
        }

        GUI.BeginClip(clip, offset, Vector2.zero, false);

        bool toggle = false;

        if (isTransitioning)
        {
            GUILayout.BeginHorizontal();
            RenderFolders(categories.Keys, clip.position, searchRect.width);
            GUILayout.Space(-5f);
            RenderContents(categories[selectedCategory], null, clip.position, searchRect.width, selectedCategory);
            GUILayout.EndHorizontal();
        }
        else
        {
            if (folderOverview)
            {
                selectedCategory = RenderFolders(categories.Keys, clip.position, searchRect.width);
                toggle = !String.IsNullOrEmpty(selectedCategory);
            }
            else
            {
                Tuple<bool, Type> t = RenderContents(categories[selectedCategory], null, clip.position, searchRect.width, selectedCategory);
                toggle = t.Item1;
            }
        }

        if (toggle)
        {
            folderOverview = !folderOverview;
            isTransitioning = true;
            transitionStartTime = timeSinceStartup;
            offset = new Vector2(folderOverview ? -searchRect.width : 0f, 0f);
        }

        GUI.EndClip();

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
    private static string RenderFolders(IEnumerable<string> folders, Vector2 offset, float contentWidth)
    {
        GUILayout.BeginVertical(GUILayout.Width(contentWidth));

        string s = null;

        foreach (string folder in folders)
        {
            if (folder == "")
                continue;

            Vector2 iconSize = EditorGUIUtility.GetIconSize();
            EditorGUIUtility.SetIconSize(new Vector2(16, 16));

            GUIContent content = new GUIContent(folder, Styles.folder);

            Rect r = GUILayoutUtility.GetRect(content, GUI.skin.label, GUILayout.Height(32));
            r = new Rect(r.position - offset, r.size);

            GUI.Label(r, content);

            Vector2 b = EditorStyles.iconButton.CalcSize(new GUIContent(Styles.next));

            if (GUI.Button(new Rect(r.xMax - b.x - 5f, r.y + r.height / 2f - b.y / 2f, b.x, b.y), new GUIContent(Styles.next), EditorStyles.iconButton))
                s = folder;

            EditorGUIUtility.SetIconSize(iconSize);
        }

        GUILayout.EndVertical();

        return s;
    }
    private static Tuple<bool, Type> RenderContents(IEnumerable<Type> contents, Type selected, Vector2 contentOffset, float contentWidth, string title)
    {
        GUILayout.BeginVertical(GUILayout.Width(contentWidth));

        Vector2 iconSize = EditorGUIUtility.GetIconSize();
        EditorGUIUtility.SetIconSize(new Vector2(24, 24));

        GUIContent t = new GUIContent(title, Styles.folderOpen);
        Rect r = GUILayoutUtility.GetRect(t, EditorStyles.boldLabel);
        r = new Rect(r.position - contentOffset + new Vector2(16, 0), r.size);

        EditorGUIUtility.SetIconSize(new Vector2(16, 16));

        Vector2 b = EditorStyles.iconButton.CalcSize(new GUIContent(Styles.prev));

        bool goBack = GUI.Button(new Rect(r.x - 16, r.y + r.height / 2f - b.y / 2f, b.x, b.y), new GUIContent(Styles.prev), EditorStyles.iconButton);

        EditorGUIUtility.SetIconSize(new Vector2(24, 24));

        GUI.Label(r, t, EditorStyles.boldLabel);

        EditorGUIUtility.SetIconSize(new Vector2(16, 16));

        int i = 0;

        foreach (Type type in contents)
        {
            bool isSelected = type == selected;

            nodeIcons.TryGetValue(type, out Texture2D icon);

            if (icon == null)
                icon = nodeIcons[type] = Schema.Node.GetNodeIcon(type);

            GUIContent c = new GUIContent(type.Name, icon);

            r = GUILayoutUtility.GetRect(c, GUI.skin.label, GUILayout.Height(32));
            r = new Rect(r.position - contentOffset + new Vector2(16, 0), r.size);

            Color col = GUI.color;

            if (favorites.Contains(type.FullName))
                GUI.color = EditorGUIUtility.isProSkin ? new Color32(252, 191, 7, 255) : new Color32(199, 150, 0, 255);
            else
                GUI.color = Styles.windowAccent;

            if (GUI.Button(new Rect(r.x - 16, r.y + r.height / 2f - 8, 16, 16), Styles.favorite, GUIStyle.none))
            {
                if (favorites.Contains(type.FullName))
                    favorites.Remove(type.FullName);
                else
                    favorites.Add(type.FullName);

                SchemaEditor.NodeEditor.NodeEditorPrefs.SetList("SCHEMA_PREF__favorites", favorites);
            }

            GUI.color = col;

            if (isSelected)
                EditorGUI.DrawRect(r, GUI.skin.settings.selectionColor);

            GUI.Label(r, c);
            i++;
        }

        EditorGUIUtility.SetIconSize(iconSize);

        GUILayout.EndVertical();

        return new Tuple<bool, Type>(goBack, null);
    }
}