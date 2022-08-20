using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SchemaEditor.Utilities
{
    public static class EditorHelperMethods
    {
        private static void ShowAsContext(this GenericMenu menu, Vector2 position)
        {
            Vector2 cache = Event.current.mousePosition;
            Event.current.mousePosition = position;
            menu.ShowAsContext();
            Event.current.mousePosition = cache;
        }

        public static void ShowAsContext(this GenericMenu menu, float x, float y)
        {
            menu.ShowAsContext(new Vector2(x, y));
        }

        public static void Move<T>(this List<T> list, T item, int newIndex)
        {
            if (item != null)
            {
                int oldIndex = list.IndexOf(item);
                if (oldIndex > -1)
                {
                    list.RemoveAt(oldIndex);

                    if (newIndex > oldIndex) newIndex--;
                    // the actual index could have shifted due to the removal

                    list.Insert(newIndex, item);
                }
            }
        }

        public static void Move<T>(this T[] array, T item, int newIndex)
        {
            if (item != null)
            {
                int oldIndex = Array.IndexOf(array, item);

                if (oldIndex > -1)
                {
                    ArrayUtility.RemoveAt(ref array, oldIndex);

                    if (newIndex > oldIndex) newIndex--;

                    ArrayUtility.Insert(ref array, newIndex, item);
                }
            }
        }

        public static void DrawIfRepaint(this GUIStyle style, Rect position, bool isHover, bool isActive, bool on,
            bool hasKeyboardFocus)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            style.Draw(position, isHover, isActive, on, hasKeyboardFocus);
        }

        public static void DrawIfRepaint(this GUIStyle style, Rect position, GUIContent content, bool isHover,
            bool isActive, bool on, bool hasKeyboardFocus)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            style.Draw(position, content, isHover, isActive, on, hasKeyboardFocus);
        }

        public static void AddItem(this GenericMenu menu, GUIContent content, bool on, GenericMenu.MenuFunction func,
            bool disabled)
        {
            if (disabled)
                menu.AddDisabledItem(content, on);
            else
                menu.AddItem(content, on, func);
        }

        public static void AddItem(this GenericMenu menu, string content, bool on, GenericMenu.MenuFunction func,
            bool disabled)
        {
            menu.AddItem(new GUIContent(content), on, func, disabled);
        }
    }
}