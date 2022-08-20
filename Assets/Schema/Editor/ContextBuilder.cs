using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace SchemaEditor.Internal
{
    public class ContextBuilder
    {
        public ContextBuilder()
        {
            menu = new GenericMenu();
        }

        public GenericMenu menu { get; }

        public void AddItem(string content, GenericMenu.MenuFunction menuFunction)
        {
            AddItem(new GUIContent(content), false, menuFunction);
        }

        public void AddItem(string content, GenericMenu.MenuFunction menuFunction, bool disabled)
        {
            AddItem(new GUIContent(content), false, menuFunction, disabled);
        }

        public void AddSeparator()
        {
            AddSeparator("");
        }

        public void AddSeparator(string path)
        {
            menu.AddSeparator(path);
        }

        public void AddItem(GUIContent content, bool on, GenericMenu.MenuFunction menuFunction)
        {
            menu.AddItem(content, on, menuFunction);
        }

        public void AddItem(GUIContent content, bool on, GenericMenu.MenuFunction menuFunction, bool disabled)
        {
            if (disabled)
                menu.AddDisabledItem(content, on);
            else
                menu.AddItem(content, on, menuFunction);
        }

        public void AddItem(GUIContent content, bool on, GenericMenu.MenuFunction2 menuFunction, object userData)
        {
            menu.AddItem(content, on, menuFunction, userData);
        }

        public void AddItem(GUIContent content, bool on, GenericMenu.MenuFunction2 menuFunction, object userData,
            bool disabled)
        {
            if (disabled)
                menu.AddDisabledItem(content, on);
            else
                menu.AddItem(content, on, menuFunction, userData);
        }

        public void AddShortcut(string shortcut, GenericMenu.MenuFunction menuFunction, string nameInMenu = "")
        {
            AddShortcut(shortcut, menuFunction, false, nameInMenu);
        }

        public void AddShortcut(string shortcut, GenericMenu.MenuFunction menuFunction, bool disabled,
            string nameInMenu = "")
        {
            int pos = shortcut.LastIndexOf("/") + 1;
            string text = string.IsNullOrEmpty(nameInMenu) ? shortcut.Substring(pos) : nameInMenu;

            KeyCombination def = GetCommandKeyCombination(shortcut);
            string keyCode = GetMenuKeyFromKeyCode(def.keyCode);

            if (!string.IsNullOrEmpty(keyCode))
            {
                if (def.action || def.alt || def.shift)
                    text += $" {(def.action ? "%" : "")}{(def.shift ? "#" : "")}{(def.alt ? "&" : "")}{keyCode}";
                else
                    text += $" _{keyCode}";
            }

            AddItem(text, menuFunction, disabled);
        }

        private KeyCombination GetCommandKeyCombination(string commandName)
        {
            IEnumerable<KeyCombination> sequence =
                UnityEditor.ShortcutManagement.ShortcutManager.instance.GetShortcutBinding(commandName)
                    .keyCombinationSequence;

            KeyCombination defaultKeyCombination = new KeyCombination(KeyCode.None);

            if (sequence.Count() > 0)
                return UnityEditor.ShortcutManagement.ShortcutManager.instance.GetShortcutBinding(commandName)
                    .keyCombinationSequence.ElementAt(0);
            return defaultKeyCombination;
        }

        private string GetMenuKeyFromKeyCode(KeyCode code)
        {
            //Return the raw keycode if keycode is a letter
            string codeStr = code.ToString();
            if (Regex.IsMatch(codeStr, @"^[a-zA-Z]$")) return codeStr.ToLower();

            //Otherwise, manually return keycodes
            switch (code)
            {
                case KeyCode.None:
                    return "";
                case KeyCode.Home:
                    return "HOME";
                case KeyCode.PageUp:
                    return "PGUP";
                case KeyCode.PageDown:
                    return "PGDN";
                case KeyCode.End:
                    return "END";
                case KeyCode.UpArrow:
                    return "UP";
                case KeyCode.LeftArrow:
                    return "LEFT";
                case KeyCode.RightArrow:
                    return "RIGHT";
                case KeyCode.DownArrow:
                    return "DOWN";
                case KeyCode.Insert:
                    return "INS";
                case KeyCode.Delete:
                    return "DEL";
                case KeyCode.Alpha0:
                case KeyCode.Alpha1:
                case KeyCode.Alpha2:
                case KeyCode.Alpha3:
                case KeyCode.Alpha4:
                case KeyCode.Alpha5:
                case KeyCode.Alpha6:
                case KeyCode.Alpha7:
                case KeyCode.Alpha8:
                case KeyCode.Alpha9:
                    return codeStr.Substring(5);
                case KeyCode.Backslash:
                    return "\\";
                case KeyCode.Slash:
                    return "/";
                case KeyCode.Period:
                    return ".";
                case KeyCode.BackQuote:
                    return "`";
                case KeyCode.LeftBracket:
                    return "[";
                case KeyCode.RightBracket:
                    return "]";
                case KeyCode.Minus:
                    return "-";
                case KeyCode.Equals:
                    return "=";
                case KeyCode.Semicolon:
                    return ";";
                case KeyCode.Quote:
                    return "\'";
                case KeyCode.Comma:
                    return ",";
                case KeyCode.F1:
                case KeyCode.F2:
                case KeyCode.F3:
                case KeyCode.F4:
                case KeyCode.F5:
                case KeyCode.F6:
                case KeyCode.F7:
                case KeyCode.F8:
                case KeyCode.F9:
                case KeyCode.F10:
                case KeyCode.F11:
                case KeyCode.F12:
                case KeyCode.F13:
                case KeyCode.F14:
                case KeyCode.F15:
                    return codeStr.Substring(2);
            }

            //Cases where nothing is rendered to the menu
            return "";
        }
    }
}