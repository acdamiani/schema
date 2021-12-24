using UnityEngine;
using UnityEditor;

namespace Schema.Editor.Utilities
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
    }
}