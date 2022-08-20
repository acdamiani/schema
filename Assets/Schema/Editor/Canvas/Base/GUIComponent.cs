using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SchemaEditor.Internal.ComponentSystem
{
    [Serializable]
    public abstract class GUIComponent
    {
        public ComponentCanvas canvas { get; set; }
        public int layer { get; set; }
        public bool isDestroyed { get; private set; }

        public abstract void OnGUI();
        public abstract void Create(CreateArgs args);

        public virtual bool IsHoverable()
        {
            return true;
        }

        public virtual bool ShouldHover(Vector2 mousePosition)
        {
            return false;
        }

        public virtual bool ResolveObject(Object obj)
        {
            return false;
        }

        public virtual string GetDebugInfo()
        {
            return string.Empty;
        }

        public static void Destroy(GUIComponent component)
        {
            component.OnDestroy();
            component.canvas.Remove(component);
            component.isDestroyed = true;
        }

        protected virtual void OnDestroy()
        {
        }
    }
}