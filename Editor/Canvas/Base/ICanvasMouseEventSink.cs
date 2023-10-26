using UnityEngine;

namespace SchemaEditor.Internal.ComponentSystem
{
    public interface ICanvasMouseEventSink
    {
        Rect GetRect();
    }
}