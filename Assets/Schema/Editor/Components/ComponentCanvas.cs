using System;
using System.Collections.Generic;
using SchemaEditor.Internal;
using Schema;
using UnityEngine;
using UnityEditor;

namespace SchemaEditor.Internal
{
    [Serializable]
    public class ComponentCanvas
    {
        [SerializeField] private List<GUIComponent> components = new List<GUIComponent>();
        public void Create<T>(CreateArgs args) where T : GUIComponent, new()
        {
            T component = new T();
            component.Create(args);
            components.Add(component);
        }
        public void Draw()
        {
            foreach (GUIComponent component in components)
                component.OnGUI();
        }
    }
}