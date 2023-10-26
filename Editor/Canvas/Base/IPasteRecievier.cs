using UnityEngine;

namespace SchemaEditor.Internal.ComponentSystem
{
    public interface IPasteRecievier
    {
        bool IsPastable();
        Object GetReciever();
    }
}