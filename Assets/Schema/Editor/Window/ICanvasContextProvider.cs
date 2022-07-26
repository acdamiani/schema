using UnityEngine;

namespace SchemaEditor.Internal
{
    public interface ICanvasContextProvider
    {
        int GetControlID();
        Rect GetRect();
        Rect GetZoomRect();
    }
}