using System;
using System.Collections.Generic;
using System.Linq;
using Schema;
using Schema.Utilities;
using SchemaEditor.Utilities;
using UnityEditor;
using UnityEngine;

namespace SchemaEditor.Editors
{
    [CustomEditor(typeof(Node)), CanEditMultipleObjects]
    public class DefaultNodeEditor : Editor
    {
        private readonly CacheDictionary<Type, bool> allowedOne = new CacheDictionary<Type, bool>();

        private readonly CacheDictionary<Type, IEnumerable<Type>> disabled =
            new CacheDictionary<Type, IEnumerable<Type>>();

        private readonly CacheDictionary<Type, Texture2D> icons = new CacheDictionary<Type, Texture2D>();
        private SerializedProperty comment;
        private SerializedProperty enableStatusIndicator;
        private SerializedProperty modifiers;
        private Node node;
        private SerializedProperty nodeName;

        private void OnEnable()
        {
            if (targets.Any(x => x == null))
            {
                DestroyImmediate(this);
                return;
            }

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
            if (targets.Length == 1 && target.GetType() != typeof(Root))
                DoModifers(node.modifiers);
            EditorGUILayout.PropertyField(enableStatusIndicator);

            serializedObject.ApplyModifiedProperties();
        }

        private GenericMenu AddModifierMenu()
        {
            IEnumerable<Type> types = HelperMethods.GetEnumerableOfType(typeof(Modifier));

            GenericMenu menu = new GenericMenu();

            foreach (Type t in types)
            {
                Texture2D icon = icons.GetOrCreate(t, () => Modifier.GetModifierIcon(t));

                IEnumerable<Type> disallowed = disabled.GetOrCreate(t, () => Modifier.DisallowedTypes(t));

                bool isAllowedOne = allowedOne.GetOrCreate(t, () => Modifier.AllowedOne(t)) &&
                                    node.modifiers.Any(x => x.GetType() == t);
                bool isDisallowed = disallowed.Count() > 0 && node.modifiers.Any(x => disallowed.Contains(x.GetType()));

                menu.AddItem(
                    new GUIContent(t.Name, icon),
                    false,
                    () => node.AddModifier(t),
                    isAllowedOne || isDisallowed
                );
            }

            return menu;
        }

        private void DoModifers(IEnumerable<Modifier> modifiers)
        {
            Rect r;

            EditorGUILayout.LabelField("Modifiers");

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
                SchemaGUI.DrawRotatedTexture(r, Icons.GetResource("foldout", false), m.expanded ? 90f : 0f);
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
                    GUILayout.Button(new GUIContent(Icons.GetResource("move_up", false), "Move Modifier Up"),
                        EditorStyles.miniButtonMid,
                        GUILayout.ExpandWidth(false))
                    && i > 0
                )
                    node.MoveModifier(m, i - 1);

                if (
                    GUILayout.Button(new GUIContent(Icons.GetResource("move_down", false), "Move Modifier Down"),
                        EditorStyles.miniButtonRight, GUILayout.ExpandWidth(false))
                    && i < modifiers.Count() - 1
                )
                    node.MoveModifier(m, i + 1);

                EditorGUIUtility.SetIconSize(Vector2.one * 12f);

                r = GUILayoutUtility.GetRect(12f, 12f, GUIStyle.none, GUILayout.ExpandWidth(false));
                r.y += 6f;

                GUI.color = new Color(0.75f, 0.75f, 0.75f, 1f);

                if (GUI.Button(r, new GUIContent(Icons.GetResource("close", false), "Remove Modifier"), GUIStyle.none))
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
}