using UnityEngine;
using UnityEditor;

namespace SchemaEditor.Internal
{
    public interface ICanvasContextProvider
    {
        int GetControlID();
        ComponentCanvas GetCanvas();
        Rect GetRect();
        Rect GetViewRect();
        float GetToolbarHeight();
        EditorWindow GetEditorWindow();
    }
}