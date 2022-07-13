using UnityEngine;

namespace SchemaEditor.Internal
{
    public interface ISelectable
    {
        bool IsSelectable();
        bool IsHit(Vector2 mousePosition);
        bool Overlaps(Rect rect);
        void Select(bool additive);
        void Deselect();
    }
}