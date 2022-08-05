using System;
using System.Collections.Generic;
using SchemaEditor.Internal.ComponentSystem;
using SchemaEditor.Internal.ComponentSystem.Components;
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
        public SelectionBoxComponent selectionBoxComponent { get; }
        public DebugViewComponent debugViewComponent { get; }
        public MinimapComponent minimapComponent { get; }
        public ICanvasContextProvider context { get; }
        public PannerZoomer zoomer { get; }
        public Vector2 mousePositionNoZoom { get { return _mousePositionNoZoom; } }
        private Vector2 _mousePositionNoZoom;
        private Action<Rect, float, Vector2> _doGrid;
        public MouseCursor cursor { get; set; }
        public delegate void OnComponentListModifiedHandler();
        public event OnComponentListModifiedHandler onComponentListModified;
        public ComponentCanvas(
            ICanvasContextProvider context,
            SelectionBoxComponent.SelectionBoxComponentCreateArgs selectionBoxComponentCreateArgs,
            MinimapComponent.MinimapComponentCreateArgs minimapComponentCreateArgs,
            PannerZoomer zoomer,
            Action<Rect, float, Vector2> doGrid
        )
        {
            selectionBoxComponentCreateArgs.layer = 1;
            selectionBoxComponent = Create<SelectionBoxComponent>(selectionBoxComponentCreateArgs);
            selectionBoxComponent.hidden = true;

            CreateArgs empty = new CreateArgs();
            empty.layer = 50;

            debugViewComponent = Create<DebugViewComponent>(empty);

            minimapComponentCreateArgs.layer = 100;
            minimapComponent = Create<MinimapComponent>(minimapComponentCreateArgs);

            BlockerComponent.BlockerComponentCreateArgs blockerComponentCreateArgs = new BlockerComponent.BlockerComponentCreateArgs();
            blockerComponentCreateArgs.rect = () => new Rect(0f, 0f, context.GetViewRect().width, context.GetToolbarHeight());
            Create<BlockerComponent>(blockerComponentCreateArgs);

            this.zoomer = zoomer;
            this.context = context;
            this._doGrid = doGrid;
        }
        public T Create<T>(CreateArgs args) where T : GUIComponent, new()
        {
            T component = new T();
            ArrayUtility.Add(ref _components, component);
            ((GUIComponent)component).canvas = this;
            component.Create(args);
            component.layer = args.layer;

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
                Vector2 mouse = component is IViewElement ? mousePosition * zoomer.zoom : mousePosition;

                if (component.IsHoverable() && component.ShouldHover(mouse))
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
        public void Reset()
        {
            ArrayUtility.Clear(ref _components);
            ArrayUtility.Clear(ref _selected);

            selectionBoxComponent.hidden = true;
        }
        public void Draw()
        {
            if (
                (Event.current.isKey || Event.current.isMouse || Event.current.isScrollWheel)
                && !context.GetViewRect().Contains(Event.current.mousePosition)
            )
                return;

            // EditorGUIUtility.AddCursorRect(context.GetRect(), cursor);

            _doGrid(context.GetViewRect(), zoomer.zoom, zoomer.pan);

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
            {
                if (!IsInSink(Event.current) || viewComponents[i] is ICanvasMouseEventSink)
                    viewComponents[i].OnGUI();
            }

            zoomer.End();

            for (int i = c.Length - 1; i >= 0; i--)
            {
                if (!IsInSink(Event.current) || c[i] is ICanvasMouseEventSink)
                    c[i].OnGUI();
            }
        }
        private Vector2 lastMouse;
        public GUIComponent hovered
        {
            get
            {
                if (_mousePositionNoZoom != lastMouse)
                {
                    lastMouse = _mousePositionNoZoom;
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
            if (IsInSink(Event.current))
                return;

            switch (Event.current.rawType)
            {
                case EventType.MouseDown:
                    OnMouseDown(Event.current, layeredComponents);
                    break;
                case EventType.MouseDrag:
                    OnMouseDrag(Event.current);
                    break;
                case EventType.ScrollWheel:
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
                    DeleteSelected();
                    break;
            }
        }
        private bool IsInSink(Event mouseEvent)
        {
            if (!mouseEvent.isMouse && !mouseEvent.isKey && !mouseEvent.isScrollWheel)
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

                        Vector2 mouse = component is IViewElement ? mouseEvent.mousePosition * zoomer.zoom : mouseEvent.mousePosition;

                        if (
                            selectableComponent == null
                                || !selectableComponent.IsSelectable()
                                || !selectableComponent.IsHit(mouse)
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
                case 1:
                    GenericMenu menu = BuildContextMenu();

                    menu.ShowAsContext();

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
            zoomer.zoom += mouseEvent.delta.y * 0.035f;

            if (!selectionBoxComponent.hidden)
                return;
        }
        public void DoBoxOverlap(Rect boxRect, Event current)
        {
            for (int i = components.Length - 1; i >= 0; i--)
            {
                GUIComponent component = components[i];
                ISelectable selectableComponent = component as ISelectable;

                Rect rect = component is IViewElement ? new Rect(boxRect.position * zoomer.zoom, boxRect.size * zoomer.zoom) : boxRect;

                rect = rect.Normalize();

                bool componentIsSelected = ArrayUtility.Contains(_selected, component);

                if (
                    selectableComponent == null
                        || !selectableComponent.IsSelectable()
                        || !selectableComponent.Overlaps(rect)
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
        private void DeleteSelected()
        {
            foreach (GUIComponent component in components.Where(x => x is IDeletable))
            {
                IDeletable deletable = (IDeletable)component;

                if (deletable.IsDeletable())
                {
                    deletable.Delete();

                    ArrayUtility.Remove(ref _components, component);

                    if (_selected.Contains(component))
                        ArrayUtility.Remove(ref _selected, component);
                }
            }
        }
        private void DoCopy()
        {
            IEnumerable<NodeComponent> selectedNodes = selected
                .Where(x => x is NodeComponent)
                .Cast<NodeComponent>();

            if (selectedNodes.Count() == 0)
                return;

            Copy.DoCopy(this, selectedNodes);
        }
        private GenericMenu BuildContextMenu()
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem("Delete", false, DeleteSelected, false);
            menu.AddItem("Copy", false, DoCopy, false);

            return menu;
        }
    }
}