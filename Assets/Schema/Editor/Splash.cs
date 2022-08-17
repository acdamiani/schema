using System;
using System.Collections.Generic;
using System.IO;
using Schema;
using SchemaEditor;
using SchemaEditor.Internal.ComponentSystem;
using SchemaEditor.Utilities;
using UnityEditor;
using UnityEngine;

public class Splash : IWindowComponentProvider
{
    private readonly Dictionary<string, string> recentFiles;
    private Rect rect;
    private Vector2 scroll;
    private string selected = string.Empty;

    public Splash()
    {
        recentFiles = new Dictionary<string, string>();

        string[] recent = EditorPrefs.GetString("Schema Recently Opened", "")
            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

        for (int j = 0; j < recent.Length; j++)
        {
            string path = AssetDatabase.GUIDToAssetPath(recent[j]);

            if (!File.Exists(path))
                continue;

            string n = Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(recent[j]));
            recentFiles[recent[j]] = n;
        }

        EditorPrefs.SetString("Schema Recently Opened", string.Join(",", recentFiles.Keys));
    }

    public void HandleWinInfo(Rect rect, GUIContent title, GUIStyle style)
    {
        this.rect = rect;
    }

    public bool ShouldClose()
    {
        return false;
    }

    public void OnGUI(int id)
    {
        Event current = Event.current;
        Vector2 mousePosition = Event.current.mousePosition;

        Texture2D splash = Icons.GetResource("splash");

        GUILayout.Label(splash, GUIStyle.none);

        GUILayout.Label("Recent Files", EditorStyles.largeLabel);

        scroll = GUILayout.BeginScrollView(scroll);

        GUIContent c;

        foreach (KeyValuePair<string, string> s in recentFiles)
        {
            c = new GUIContent(s.Value);

            Rect r = GUILayoutUtility.GetRect(c, Styles.blackboardEntry);

            bool isHovered = r.Contains(current.mousePosition);

            if (isHovered && current.rawType == EventType.MouseDown && current.button == 0)
                selected = s.Key;

            bool isSelected = selected == s.Key;

            Styles.blackboardEntry.DrawIfRepaint(r, c, isHovered, false, isSelected, false);
        }

        GUILayout.EndScrollView();

        GUILayout.FlexibleSpace();

        GUILayout.BeginVertical(GUILayout.Height(EditorStyles.toolbar.fixedHeight));

        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();

        GUILayout.FlexibleSpace();

        EditorGUI.BeginDisabledGroup(!recentFiles.ContainsKey(selected));
        if (GUILayout.Button("Load", EditorStyles.miniButtonLeft, GUILayout.Width(100f)))
            NodeEditor.OpenGraph(AssetDatabase.LoadAssetAtPath<Graph>(
                AssetDatabase.GUIDToAssetPath(selected)));
        EditorGUI.EndDisabledGroup();

        if (GUILayout.Button("New Graph", EditorStyles.miniButtonRight, GUILayout.Width(100f)))
            CreateNew();

        GUILayout.FlexibleSpace();

        GUILayout.EndHorizontal();

        GUILayout.Space(8f);

        GUILayout.EndVertical();
    }

    private void CreateNew()
    {
        Graph graph = ScriptableObject.CreateInstance<Graph>();

        string path = AssetDatabase.GenerateUniqueAssetPath("Assets/NewGraph.asset");

        AssetDatabase.CreateAsset(graph, path);
        NodeEditor.OpenGraph(graph);
    }
}