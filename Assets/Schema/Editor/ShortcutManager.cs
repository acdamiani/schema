using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public static class ShortcutManager
{
    public static List<Shortcut> shortcuts;

    public static void AddShortcut(Shortcut shortcut)
    {
        if (shortcuts == null)
            shortcuts = new List<Shortcut>();

        if (!shortcuts.Contains(shortcut))
            shortcuts.Add(shortcut);
    }

    public static void AddShortcut(KeyCode keyCode, EventModifiers modifiers, UnityAction action)
    {
        Shortcut shortcut = new Shortcut();

        shortcut.keyCode = keyCode;
        shortcut.modifiers = modifiers;
        shortcut.action = action;

        AddShortcut(shortcut);
    }

    public static void KeyPress(Event current)
    {
        if (shortcuts == null)
            shortcuts = new List<Shortcut>();

        if (Event.current.type == EventType.KeyDown)
        {
            List<Shortcut> valid = shortcuts.FindAll(shortcut =>
                shortcut.keyCode == current.keyCode && shortcut.modifiers == current.modifiers);
            valid.ForEach(shortcut => shortcut.action.Invoke());

            if (valid.Count > 0) current.Use();
        }
    }

    public static void ClearShortcuts()
    {
        if (shortcuts == null)
            shortcuts = new List<Shortcut>();

        shortcuts.Clear();
    }

    [Serializable]
    public struct Shortcut
    {
        public EventModifiers modifiers;
        public KeyCode keyCode;
        public UnityAction action;
    }
}