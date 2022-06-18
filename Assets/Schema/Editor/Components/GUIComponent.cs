using Schema;
using System;
using UnityEngine;

namespace SchemaEditor.Internal
{
    [Serializable]
    public abstract class GUIComponent
    {
        public abstract void OnGUI();
        protected abstract Rect GetRect();
        public abstract void Create(CreateArgs args);
        public void SetDirty() { isDirty = true; }
        protected bool GetAndApply()
        {
            bool dirty = isDirty;
            isDirty = false;

            return dirty;
        }
        public Rect rect
        {
            get
            {
                if (_rect == null || GetAndApply())
                    _rect = GetRect();

                Rect r = _rect.Value;
                r.position = NodeEditor.GridToWindowPositionNoClipped(r.position);

                return r;
            }
        }
        private Rect? _rect;
        protected bool isDirty { get; private set; }
    }
    public abstract class CreateArgs { }
}