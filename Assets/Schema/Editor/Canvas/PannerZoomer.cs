using UnityEngine;

namespace SchemaEditor.Internal.ComponentSystem
{
    public class PannerZoomer
    {
        public float zoomSpeed { get { return _zoomSpeed; } }
        private float _zoomSpeed;
        public float zoom { get { return _zoom; } }
        private float _zoom;
        public Vector2 pan { get { return _pan; } }
        private Vector2 _pan;
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
            this._zoomSpeed = zoomSpeed;
            this._zoom = initialZoom;
            this._pan = initialPan;
            this._topOffset = topOffset;

            this.context = context;
        }
        public void Begin()
        {
            if (Event.current.rawType == EventType.ScrollWheel)
            {
                _zoom += Event.current.delta.y * zoomSpeed;
                _zoom = Mathf.Clamp(_zoom, 1f, 2.5f);

                onZoomChange?.Invoke(_zoom);
            }
            else if (Event.current.rawType == EventType.MouseDrag && Event.current.button == 2)
            {
                _pan += Event.current.delta * zoom;

                onPanChange?.Invoke(_pan);
            }

            prevMatrix = GUI.matrix;
            GUI.EndClip();

            Rect zoomable = context.GetZoomRect();

            GUIUtility.ScaleAroundPivot(Vector2.one / zoom, zoomable.size * 0.5f);
            GUI.BeginClip(new Rect(-((zoomable.width * zoom) - zoomable.width) * 0.5f, -(((zoomable.height * zoom) - zoomable.height) * 0.5f) + (_topOffset() * zoom),
                zoomable.width * zoom,
                zoomable.height * zoom));
        }
        public void End()
        {
            GUI.EndClip();
            GUI.BeginClip(context.GetRect());
            GUI.matrix = prevMatrix;
        }
        public Vector2 GridToWindowPositionNoClipped(Vector2 position)
        {
            Vector2 center = context.GetZoomRect().size * 0.5f;
            float xOffset = center.x * zoom + (pan.x + position.x);
            float yOffset = center.y * zoom + (pan.y + position.y);
            return new Vector2(xOffset, yOffset);
        }
        public Vector2 WindowToGridPosition(Vector2 position)
        {
            return (position - (context.GetZoomRect().size * 0.5f) - (pan / zoom)) * zoom;
        }
        public Vector2 GridToWindowPosition(Vector2 position)
        {
            return (context.GetZoomRect().size * 0.5f) + (pan / zoom) + (position / zoom);
        }
    }
}