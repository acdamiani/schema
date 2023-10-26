using System;
using UnityEditor;
using UnityEngine;

namespace SchemaEditor.Internal.ComponentSystem
{
    public class PannerZoomer
    {
        public delegate void OnPanChangeHandler(Vector2 pan);

        public delegate void OnZoomChangeHandler(float zoom);

        private readonly Func<float> _topOffset;
        private float _zoom;
        private Matrix4x4 prevMatrix;

        public PannerZoomer(ICanvasContextProvider context, float zoomSpeed, float initialZoom, Vector2 initialPan,
            Func<float> topOffset)
        {
            this.zoomSpeed = zoomSpeed;
            zoom = initialZoom;
            pan = initialPan;
            _topOffset = topOffset;

            this.context = context;
        }

        public float zoomSpeed { get; }

        public float zoom
        {
            get => _zoom;
            set
            {
                _zoom = Mathf.Clamp(value, 1f, 2.5f);
                onZoomChange?.Invoke(_zoom);
            }
        }

        public Vector2 pan { get; set; }
        public Rect rect => context.GetRect();
        public ICanvasContextProvider context { get; set; }
        public event OnZoomChangeHandler onZoomChange;
        public event OnPanChangeHandler onPanChange;

        public void Begin()
        {
            if ((Event.current.rawType == EventType.MouseDown && Event.current.button == 2) || (Event.current.rawType == EventType.MouseDown && Event.current.command && Event.current.button == 0))
            {
                context.GetCanvas().cursor = MouseCursor.Pan;
            }
            else if (Event.current.rawType == EventType.MouseUp)
            {
                context.GetCanvas().cursor = MouseCursor.Arrow;
            }
            else if ((Event.current.rawType == EventType.MouseDrag && Event.current.button == 2) || (Event.current.rawType == EventType.MouseDrag && Event.current.command && Event.current.button == 0))
            {
                pan += Event.current.delta * zoom;
                onPanChange?.Invoke(pan);
            }

            prevMatrix = GUI.matrix;
            GUI.EndClip();

            Rect zoomRect = context.GetViewRect();

            GUIUtility.ScaleAroundPivot(Vector2.one / zoom, zoomRect.size * 0.5f);
            GUI.BeginClip(new Rect(-(zoomRect.width * zoom - zoomRect.width) * 0.5f,
                -((zoomRect.height * zoom - zoomRect.height) * 0.5f) + _topOffset() * zoom,
                zoomRect.width * zoom,
                zoomRect.height * zoom));
        }

        public void End()
        {
            GUI.EndClip();
            GUI.BeginClip(context.GetRect());

            GUI.matrix = prevMatrix;
        }

        public Vector2 GridToWindowPositionNoClipped(Vector2 position)
        {
            Vector2 center = context.GetViewRect().size * 0.5f;
            float xOffset = center.x * zoom + (pan.x + position.x);
            float yOffset = center.y * zoom + (pan.y + position.y);
            return new Vector2(xOffset, yOffset);
        }

        public Vector2 WindowToGridPosition(Vector2 position)
        {
            return (position - context.GetViewRect().size * 0.5f - pan / zoom) * zoom;
        }

        public Vector2 GridToWindowPosition(Vector2 position)
        {
            return context.GetViewRect().size * 0.5f + pan / zoom + position / zoom;
        }

        public Rect WindowToGridRect(Rect rect)
        {
            rect.position = WindowToGridPosition(rect.position);
            rect.size *= zoom;
            return rect;
        }
    }
}
