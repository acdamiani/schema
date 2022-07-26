using Schema;
using System;
using UnityEngine;

namespace SchemaEditor.Internal
{
    [Serializable]
    public abstract class GUIComponent
    {
        public abstract void OnGUI();
        public abstract void Create(CreateArgs args);
        public virtual bool IsHoverable() { return true; }
        public virtual bool ShouldHover(Vector2 mousePosition) { return false; }
        public virtual bool ResolveObject(UnityEngine.Object obj) { return false; }
        public virtual string GetDebugInfo() { return String.Empty; }
        public static void Destroy(GUIComponent component) { component.OnDestroy(); component.canvas.Remove(component); }
        protected virtual void OnDestroy() { }
        public ComponentCanvas canvas { get; set; }
        public int layer { get; set; }
    }
    public class CreateArgs
    {
        public int layer { get; set; }
    }
}