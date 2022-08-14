using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace SchemaEditor.Internal.ComponentSystem
{
    public interface IPasteRecievier
    {
        bool IsPastable();
        Object GetReciever();
    }
}