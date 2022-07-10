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
        public virtual bool IsHovered(Vector2 mousePosition) { return false; }
    }
    public abstract class CreateArgs { }
}