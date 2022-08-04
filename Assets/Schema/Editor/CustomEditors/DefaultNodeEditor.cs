using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using Schema;
using Schema.Utilities;
using SchemaEditor;
using System.Collections.Generic;

[CustomEditor(typeof(Schema.Node)), CanEditMultipleObjects]
public class DefaultNodeEditor : Editor
{
    SerializedProperty nodeName;
    SerializedProperty enableStatusIndicator;
    SerializedProperty comment;
    SerializedProperty modifiers;
    Node node;
    private CacheDictionary<Type, bool> allowedOne = new CacheDictionary<Type, bool>();
    void OnEnable()
    {
        nodeName = serializedObject.FindProperty("m_Name");
        comment = serializedObject.FindProperty("m_comment");
        enableStatusIndicator = serializedObject.FindProperty("m_enableStatusIndicator");
        modifiers = serializedObject.FindProperty("m_modifiers");

        node = (Node)target;
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(nodeName);
        EditorGUILayout.PropertyField(comment);
        EditorGUILayout.PropertyField(enableStatusIndicator);

        if (target.GetType() != typeof(Root))
            DoModifers(node.modifiers);

        serializedObject.ApplyModifiedProperties();
    }
    private GenericMenu AddModifierMenu()
    {
        IEnumerable<Type> types = HelperMethods.GetEnumerableOfType(typeof(Modifier));

        GenericMenu menu = new GenericMenu();

        foreach (Type t in types)
        {
            Texture2D icon = Modifier.GetModifierIcon(t);

            menu.AddItem(
                new GUIContent(t.Name, icon),
                false,
                () => node.AddModifier(t),
                allowedOne.GetOrCreate(t, () => Modifier.AllowedOne(t)) && node.modifiers.Any(x => x.GetType() == t)
            );
        }

        return menu;
    }
    private void DoModifers(IEnumerable<Modifier> modifiers)
    {
        Rect r;

        GUILayout.Label("Modifiers", EditorStyles.boldLabel);

        Vector2 iconSize = EditorGUIUtility.GetIconSize();
        EditorGUIUtility.SetIconSize(Vector2.one * 16f);

        int i = 0;

        foreach (Modifier m in modifiers)
        {
            bool removed = false;

            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal(GUILayout.Height(24f));

            GUILayout.Space(2f);

            r = GUILayoutUtility.GetRect(12f, 12f, GUIStyle.none, GUILayout.ExpandWidth(false));
            r.y += 6f;

            GUI.color = new Color(0.75f, 0.75f, 0.75f, 1f);
            SchemaGUI.DrawRotatedTexture(r, Styles.foldout, m.expanded ? 90f : 0f);
            GUI.color = Color.white;

            if (GUI.Button(r, "", GUIStyle.none))
                m.expanded = !m.expanded;

            GUILayout.Space(4f);

            m.enabled = EditorGUILayout.Toggle(m.enabled, GUILayout.Width(16f));

            r = GUILayoutUtility.GetRect(16f, 16f, GUIStyle.none, GUILayout.ExpandWidth(false));
            r.y += 4f;

            if (!m.enabled)
                GUI.color = Color.gray;
            GUI.DrawTexture(r, m.icon);
            GUI.color = Color.white;

            m.name = EditorGUILayout.TextField(m.name);

            EditorGUIUtility.SetIconSize(Vector2.one * 12f);

            GUILayout.Space(-3);

            if (
                GUILayout.Button(new GUIContent(Styles.moveUp, "Move Modifier Up"), EditorStyles.miniButtonMid, GUILayout.ExpandWidth(false))
                && i > 0
            )
                node.MoveModifier(m, i - 1);

            if (
                GUILayout.Button(new GUIContent(Styles.moveDown, "Move Modifier Down"), EditorStyles.miniButtonRight, GUILayout.ExpandWidth(false))
                && i < modifiers.Count() - 1
            )
                node.MoveModifier(m, i + 1);

            EditorGUIUtility.SetIconSize(Vector2.one * 12f);

            r = GUILayoutUtility.GetRect(12f, 12f, GUIStyle.none, GUILayout.ExpandWidth(false));
            r.y += 6f;

            GUI.color = new Color(0.75f, 0.75f, 0.75f, 1f);

            if (GUI.Button(r, new GUIContent(Styles.close, "Remove Modifier"), GUIStyle.none))
            {
                node.RemoveModifier(m);
                removed = true;
            }

            GUI.color = Color.white;

            GUILayout.Space(2f);

            EditorGUILayout.EndHorizontal();

            if (m.expanded && !removed)
                ObjectEditor.DoEditor(m);

            GUILayout.EndVertical();

            i++;
        }

        EditorGUIUtility.SetIconSize(iconSize);

        bool add = GUILayout.Button("Add Modifier");

        r = GUILayoutUtility.GetLastRect();

        if (add)
            AddModifierMenu().DropDown(new Rect(r.x, r.yMax, 0f, 0f));
    }
}