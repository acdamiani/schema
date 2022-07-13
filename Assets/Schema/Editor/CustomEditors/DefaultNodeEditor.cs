using UnityEngine;
using UnityEditor;
using System.Linq;
using Schema;
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
        EditorGUILayout.PropertyField(modifiers);

        DoModifers(node.modifiers);

        serializedObject.ApplyModifiedProperties();
    }
    private void DoModifers(IEnumerable<Modifier> modifiers)
    {
        if (GUILayout.Button("Add Modifier"))
            node.AddModifier(typeof(Schema.Builtin.Modifiers.ForceStatus));

        Vector2 iconSize = EditorGUIUtility.GetIconSize();
        EditorGUIUtility.SetIconSize(Vector2.one * 16f);

        foreach (Modifier m in modifiers)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal(GUILayout.Height(24f));

            GUILayout.Space(2f);

            Rect r = GUILayoutUtility.GetRect(12f, 12f, GUIStyle.none, GUILayout.ExpandWidth(false));
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

            GUILayout.Button(new GUIContent(Styles.moveUp, "Move Modifier Up"), EditorStyles.miniButtonMid, GUILayout.ExpandWidth(false));
            GUILayout.Button(new GUIContent(Styles.moveDown, "Move Modifier Down"), EditorStyles.miniButtonRight, GUILayout.ExpandWidth(false));

            EditorGUIUtility.SetIconSize(Vector2.one * 12f);

            r = GUILayoutUtility.GetRect(12f, 12f, GUIStyle.none, GUILayout.ExpandWidth(false));
            r.y += 6f;

            GUI.color = new Color(0.75f, 0.75f, 0.75f, 1f);
            GUI.Button(r, new GUIContent(Styles.close, "Remove Modifier"), GUIStyle.none);
            GUI.color = Color.white;

            GUILayout.Space(2f);

            EditorGUILayout.EndHorizontal();

            if (m.expanded)
                ObjectEditor.DoEditor(m);

            GUILayout.EndVertical();
        }

        EditorGUIUtility.SetIconSize(iconSize);
    }
}