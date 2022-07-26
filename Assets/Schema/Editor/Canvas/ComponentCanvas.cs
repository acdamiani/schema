using System;
using System.Collections.Generic;
using SchemaEditor.Internal.ComponentSystem;
using SchemaEditor.Internal;
using Schema.Utilities;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace SchemaEditor.Internal
{
    public class ComponentCanvas
    {
        public GUIComponent[] components { get { return _components; } }
        private GUIComponent[] _components = Array.Empty<GUIComponent>();
        public GUIComponent[] selected { get { return _selected; } }
        private GUIComponent[] _selected = Array.Empty<GUIComponent>();
        public SelectionBoxComponent selectionBoxComponent { get { return _selectionBoxComponent; } }
        private SelectionBoxComponent _selectionBoxComponent;
        public DebugViewComponent debugViewComponent { get { return _debugViewComponent; } }
        private DebugViewComponent _debugViewComponent;
        public ICanvasContextProvider context { get; }
        public PannerZoomer zoomer { get; set; }
        public Vector2 mousePositionNoZoom { get; }
        private Vector2 _mousePositionNoZoom;
        private Action<Rect, float, Vector2> _doGrid;
        public ComponentCanvas(
            ICanvasContextProvider context,
            SelectionBoxComponent.SelectionBoxComponentCreateArgs selectionBoxComponentCreateArgs,
            PannerZoomer zoomer,
            Action<Rect, float, Vector2> doGrid
        )
        {
            selectionBoxComponentCreateArgs.layer = 1;
            _selectionBoxComponent = Create<SelectionBoxComponent>(selectionBoxComponentCreateArgs);
            _selectionBoxComponent.hidden = true;

            CreateArgs debugViewComponentArgs = new CreateArgs();
            debugViewComponentArgs.layer = 100;
            _debugViewComponent = Create<DebugViewComponent>(debugViewComponentArgs);

            this.zoomer = zoomer;
            this.context = context;
            this._doGrid = doGrid;
        }
        public T Create<T>(CreateArgs args) where T : GUIComponent, new()
        {
            T component = new T();
            ((GUIComponent)component).canvas = this;
            component.Create(args);
            component.layer = args.layer;

            ArrayUtility.Add(ref _components, component);

            return component;
        }
        public GUIComponent FindComponent(Schema.Internal.GraphObject graphObject)
        {
            return (GUIComponent)_components
                .Where(c => c is IGraphObjectProvider)
                .Cast<IGraphObjectProvider>()
                .FirstOrDefault(c => c.Equals(graphObject));
        }
        public void Remove(GUIComponent component)
        {
            ArrayUtility.Remove(ref _components, component);
        }
        public void MoveToFront(GUIComponent component)
        {
            int i = Array.IndexOf(components, component);

            if (i == -1)
                return;

            components.MoveItemAtIndexToFront(i);
        }
        private GUIComponent GetHovered(Vector2 mousePosition)
        {
            foreach (GUIComponent component in components.AsEnumerable().Reverse())
            {
                if (component.IsHoverable() && component.ShouldHover(mousePosition))
                    return component;
            }

            return null;
        }
        public GUIComponent Resolve(UnityEngine.Object obj)
        {
            foreach (GUIComponent component in components)
            {
                if (!component.ResolveObject(obj))
                    continue;

                return component;
            }

            return null;
        }
        public void Draw()
        {
            _doGrid(context.GetZoomRect(), zoomer.zoom, zoomer.pan);

            _mousePositionNoZoom = Event.current.mousePosition;

            GUIComponent[] c = (GUIComponent[])components.Clone();

            Array.Sort(
                c,
                (a, b) => Event.current.type == EventType.Repaint
                    ? b.layer - a.layer
                    : a.layer - b.layer
            );

            DoEvents(c);

            c = c.Intersect(components).ToArray();

            GUIComponent[] viewComponents = c.Where(c => c is IViewElement).ToArray();
            c = c.Except(viewComponents).ToArray();

            zoomer.Begin();

            for (int i = viewComponents.Length - 1; i >= 0; i--)
                viewComponents[i].OnGUI();

            zoomer.End();

            for (int i = c.Length - 1; i >= 0; i--)
                c[i].OnGUI();
        }
        private Vector2 lastMouse;
        public GUIComponent hovered
        {
            get
            {
                if (Event.current.mousePosition != lastMouse)
                {
                    lastMouse = Event.current.mousePosition;
                    _hovered = GetHovered(lastMouse);
                }

                return _hovered;
            }
        }
        private GUIComponent _hovered;
        public void DeselectAll()
        {
            foreach (GUIComponent component in selected)
                ((ISelectable)component).Deselect();

            ArrayUtility.Clear(ref _selected);
        }
        private void DoEvents(IEnumerable<GUIComponent> layeredComponents)
        {
            bool inSink = IsInSink(Event.current);

            switch (Event.current.rawType)
            {
                case EventType.MouseDown when !inSink:
                    OnMouseDown(Event.current, layeredComponents);
                    break;
                case EventType.MouseDrag when !inSink:
                    OnMouseDrag(Event.current);
                    break;
                case EventType.ScrollWheel when !inSink:
                    OnMouseScroll(Event.current);
                    break;
                case EventType.KeyDown:
                    OnKeyDown(Event.current);
                    break;
            }
        }
        private void OnKeyDown(Event keyEvent)
        {
            switch (keyEvent.keyCode)
            {
                case KeyCode.Delete:
                    foreach (IDeletable deletable in components.Where(x => x is IDeletable).Cast<IDeletable>())
                    {
                        if (deletable.IsDeletable())
                            deletable.Delete();
                    }

                    break;
            }
        }
        private bool IsInSink(Event mouseEvent)
        {
            if (!mouseEvent.isMouse)
                return false;

            return components
                .Where(c => c is ICanvasMouseEventSink)
                .Cast<ICanvasMouseEventSink>()
                .Any(c => c.GetRect().Contains(mouseEvent.mousePosition));
        }
        private void OnMouseDown(Event mouseEvent, IEnumerable<GUIComponent> layeredComponents)
        {
            switch (mouseEvent.button)
            {
                case 0:
                    GUIUtility.hotControl = context.GetControlID();

                    foreach (GUIComponent component in layeredComponents)
                    {
                        ISelectable selectableComponent = component as ISelectable;

                        if (
                            selectableComponent == null
                                || !selectableComponent.IsSelectable()
                                || !selectableComponent.IsHit(mouseEvent.mousePosition)
                            )
                            continue;

                        MoveToFront(component);

                        if (!mouseEvent.shift && !selected.Contains(component))
                            DeselectAll();

                        if (Array.IndexOf(_selected, component) == -1)
                            ArrayUtility.Add(ref _selected, component);

                        selectableComponent.Select(mouseEvent.shift);

                        return;
                    }

                    if (hovered == null)
                    {
                        selectionBoxComponent.mouseDownPosition = zoomer.WindowToGridPosition(mouseEvent.mousePosition);
                        selectionBoxComponent.hidden = false;
                    }

                    DeselectAll();

                    break;
            }
        }
        private void OnMouseDrag(Event mouseEvent)
        {
            if (!selectionBoxComponent.hidden)
                DoBoxOverlap(selectionBoxComponent.selectionRect, mouseEvent);
        }
        private void OnMouseScroll(Event mouseEvent)
        {
            if (!selectionBoxComponent.hidden)
                return;
        }
        public void DoBoxOverlap(Rect boxRect, Event current)
        {
            for (int i = components.Length - 1; i >= 0; i--)
            {
                GUIComponent component = components[i];
                ISelectable selectableComponent = component as ISelectable;

                bool componentIsSelected = ArrayUtility.Contains(_selected, component);

                if (
                    selectableComponent == null
                        || !selectableComponent.IsSelectable()
                        || !selectableComponent.Overlaps(boxRect)
                    )
                {
                    if (componentIsSelected)
                    {
                        ArrayUtility.Remove(ref _selected, component);
                        selectableComponent?.Deselect();
                    }

                    continue;
                }

                if (!componentIsSelected)
                {
                    components.MoveItemAtIndexToFront(i);
                    ArrayUtility.Add(ref _selected, component);
                    selectableComponent.Select(true);
                }
            }
        }
    }
}