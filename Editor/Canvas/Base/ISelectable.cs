using UnityEngine;

namespace SchemaEditor.Internal.ComponentSystem
{
    public interface ISelectable
    {
        bool IsSelected();
        bool IsSelectable();
        bool IsHit(Vector2 mousePosition);
        bool Overlaps(Rect rect);
        void Select(bool additive);
        void Deselect();
    }
}