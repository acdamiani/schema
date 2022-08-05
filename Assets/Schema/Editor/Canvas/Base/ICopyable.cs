using System.Collections.Generic;

namespace SchemaEditor.Internal.ComponentSystem
{
    public interface ICopyable
    {
        bool IsCopyable();
        GUIComponent Copy();
        void PostCopy(IDictionary<GUIComponent, GUIComponent> copied);
    }
}