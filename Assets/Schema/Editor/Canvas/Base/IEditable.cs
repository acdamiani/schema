using UnityEngine;

namespace SchemaEditor.Internal.ComponentSystem
{
    public interface IEditable
    {
        Object GetEditable();
        bool IsEditable();
    }
}