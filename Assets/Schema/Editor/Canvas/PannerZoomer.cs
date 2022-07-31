using UnityEngine;

namespace SchemaEditor.Internal.ComponentSystem
{
    public class PannerZoomer
    {
        public float zoomSpeed { get; }
        public float zoom
        {
            get { return _zoom; }
            set
            {
                _zoom = Mathf.Clamp(value, 1f, 2.5f);
                onZoomChange?.Invoke(_zoom);
            }
        }
        private float _zoom;
        public Vector2 pan { get; set; }
        public Rect rect { get { return context.GetRect(); } }
        private Matrix4x4 prevMatrix;
        public ICanvasContextProvider context { get; set; }
        private System.Func<float> _topOffset;
        public delegate void OnZoomChangeHandler(float zoom);
        public delegate void OnPanChangeHandler(Vector2 pan);
        public event OnZoomChangeHandler onZoomChange;
        public event OnPanChangeHandler onPanChange;
        public PannerZoomer(ICanvasContextProvider context, float zoomSpeed, float initialZoom, Vector2 initialPan, System.Func<float> topOffset)
        {
            this.zoomSpeed = zoomSpeed;
            this.zoom = initialZoom;
            this.pan = initialPan;
            this._topOffset = topOffset;

            this.context = context;
        }
        public void Begin()
        {
            if (Event.current.rawType == EventType.MouseDown && Event.current.button == 2)
            {
                context.GetCanvas().cursor = UnityEditor.MouseCursor.Pan;
            }
            else if (Event.current.rawType == EventType.MouseUp)
            {
                context.GetCanvas().cursor = UnityEditor.MouseCursor.Arrow;
            }
            else if (Event.current.rawType == EventType.MouseDrag && Event.current.button == 2)
            {
                pan += Event.current.delta * zoom;

                onPanChange?.Invoke(pan);
            }

            prevMatrix = GUI.matrix;
            GUI.EndClip();

            Rect zoomRect = context.GetViewRect();

            GUIUtility.ScaleAroundPivot(Vector2.one / zoom, zoomRect.size * 0.5f);
            GUI.BeginClip(new Rect(-((zoomRect.width * zoom) - zoomRect.width) * 0.5f, -(((zoomRect.height * zoom) - zoomRect.height) * 0.5f) + (_topOffset() * zoom),
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
            return (position - (context.GetViewRect().size * 0.5f) - (pan / zoom)) * zoom;
        }
        public Vector2 GridToWindowPosition(Vector2 position)
        {
            return (context.GetViewRect().size * 0.5f) + (pan / zoom) + (position / zoom);
        }
        public Rect WindowToGridRect(Rect rect)
        {
            rect.position = WindowToGridPosition(rect.position);
            rect.size *= zoom;
            return rect;
        }
    }
}