using System;
using System.Collections.Generic;
using SchemaEditor.Internal;
using Schema.Utilities;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace SchemaEditor.Internal
{
    [Serializable]
    public class ComponentCanvas
    {
        [SerializeField] private List<GUIComponent> components = new List<GUIComponent>();
        public T Create<T>(CreateArgs args) where T : GUIComponent, new()
        {
            T component = new T();
            component.Create(args);
            components.Add(component);

            return component;
        }
        public void Remove(GUIComponent component)
        {
            components.Remove(component);
        }
        public void MoveToFront(GUIComponent component)
        {
            int i = components.IndexOf(component);

            if (i == -1)
                return;

            components.MoveItemAtIndexToFront(i);
        }
        private GUIComponent GetHovered(Vector2 mousePosition)
        {
            foreach (GUIComponent component in components.AsEnumerable().Reverse())
            {
                if (component.IsHoverable() && component.IsHovered(mousePosition))
                    return component;
            }

            return null;
        }
        public void Draw()
        {
            GUIComponent[] c = components.ToArray();

            foreach (GUIComponent component in c)
                component.OnGUI();
        }
        private Vector2 lastMouse;
        public GUIComponent hovered
        {
            get
            {
                if (Event.current.mousePosition != lastMouse)
                {
                    lastMouse = Event.current.mousePosition;
                    _hovered = GetHovered(lastMouse);
                }

                return _hovered;
            }
        }
        private GUIComponent _hovered;
    }
}