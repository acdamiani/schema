using UnityEngine;

namespace SchemaEditor.Internal.ComponentSystem
{
    public interface ICopyable
    {
        bool IsCopyable();
        Object GetCopyable();
    }
}