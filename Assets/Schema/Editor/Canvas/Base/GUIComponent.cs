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
        public void Destroy() { canvas.Remove(this); }
        public ComponentCanvas canvas { get; set; }
        public int layer { get; set; }
    }
    public abstract class CreateArgs
    {
        public int layer { get; set; }
    }
}