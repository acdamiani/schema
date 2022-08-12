using System;
using SchemaEditor.Internal.ComponentSystem;
using UnityEditor;
using UnityEngine;

public sealed class SelectionBoxComponent : GUIComponent
{
    private Vector2 lastSize;
    public Vector2 mouseDownPosition { get; set; }
    public bool hideOnMouseUp { get; set; }
    public bool hidden { get; set; }
    public Rect selectionRect { get; private set; }

    public override void Create(CreateArgs args)
    {
        SelectionBoxComponentCreateArgs createArgs = args as SelectionBoxComponentCreateArgs;

        if (createArgs == null)
            throw new ArgumentException();

        hideOnMouseUp = createArgs.hideOnMouseUp;
    }

    public override void OnGUI()
    {
        if (hidden)
            return;

        Event e = Event.current;

        if (hideOnMouseUp && e.rawType == EventType.MouseUp)
            hidden = true;

        Vector2 curPos = canvas.zoomer.WindowToGridPosition(e.mousePosition);
        Vector2 size = curPos - mouseDownPosition;
        Rect r = new Rect(mouseDownPosition, size);
        r.position = canvas.zoomer.GridToWindowPosition(r.position);
        r.size /= canvas.zoomer.zoom;

        selectionRect = r;

        Handles.DrawSolidRectangleWithOutline(r, new Color(0, 0, 0, 0.1f), new Color(1, 1, 1, 0.6f));
    }

    public class SelectionBoxComponentCreateArgs : CreateArgs
    {
        public bool hideOnMouseUp { get; set; }
    }
}