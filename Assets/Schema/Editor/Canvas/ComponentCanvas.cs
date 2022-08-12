using System;
using System.Collections.Generic;
using System.Linq;
using Schema;
using Schema.Internal;
using Schema.Utilities;
using SchemaEditor.Internal.ComponentSystem;
using SchemaEditor.Internal.ComponentSystem.Components;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SchemaEditor.Internal
{
    public class ComponentCanvas
    {
        public delegate void OnComponentListModifiedHandler();

        private readonly Action<Rect, float, Vector2> _doGrid;

        private GUIComponent[] _components = Array.Empty<GUIComponent>();
        private GUIComponent _hovered;
        private GUIComponent[] _selected = Array.Empty<GUIComponent>();
        private Vector2 lastMouse;

        public ComponentCanvas(
            ICanvasContextProvider context,
            SelectionBoxComponent.SelectionBoxComponentCreateArgs selectionBoxComponentCreateArgs,
            MinimapComponent.MinimapComponentCreateArgs minimapComponentCreateArgs,
            PannerZoomer zoomer,
            Action<Rect, float, Vector2> doGrid
        )
        {
            if (selectionBoxComponentCreateArgs != null)
            {
                selectionBoxComponentCreateArgs.layer = 1;
                selectionBoxComponent = Create<SelectionBoxComponent>(selectionBoxComponentCreateArgs);
                selectionBoxComponent.hidden = true;
            }

            CreateArgs empty = new();
            empty.layer = 50;

            debugViewComponent = Create<DebugViewComponent>(empty);

            if (minimapComponentCreateArgs != null)
            {
                minimapComponentCreateArgs.layer = 100;
                minimapComponent = Create<MinimapComponent>(minimapComponentCreateArgs);
            }

            BlockerComponent.BlockerComponentCreateArgs blockerComponentCreateArgs = new();
            blockerComponentCreateArgs.rect =
                () => new Rect(0f, 0f, context.GetViewRect().width, context.GetToolbarHeight());
            Create<BlockerComponent>(blockerComponentCreateArgs);

            this.zoomer = zoomer;
            this.context = context;
            _doGrid = doGrid;
        }

        public GUIComponent[] components => _components;
        public GUIComponent[] selected => _selected;
        public SelectionBoxComponent selectionBoxComponent { get; }
        public DebugViewComponent debugViewComponent { get; }
        public MinimapComponent minimapComponent { get; }
        public ICanvasContextProvider context { get; }
        public PannerZoomer zoomer { get; }
        public Vector2 mousePositionNoZoom { get; private set; }

        public MouseCursor cursor { get; set; }

        public GUIComponent hovered
        {
            get
            {
                if (mousePositionNoZoom != lastMouse)
                {
                    lastMouse = mousePositionNoZoom;
                    _hovered = GetHovered(lastMouse);
                }

                return _hovered;
            }
        }

        public event OnComponentListModifiedHandler onComponentListModified;

        public T Create<T>(CreateArgs args) where T : GUIComponent, new()
        {
            T component = new();
            ArrayUtility.Add(ref _components, component);
            component.canvas = this;
            component.Create(args);
            component.layer = args.layer;

            return component;
        }

        public GUIComponent FindComponent(GraphObject graphObject)
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

        public GUIComponent Resolve(Object obj)
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

            EditorGUIUtility.AddCursorRect(context.GetRect(), cursor);

            if (_doGrid != null)
            {
                if (zoomer != null)
                    _doGrid(context.GetViewRect(), zoomer.zoom, zoomer.pan);
                else
                    _doGrid(context.GetViewRect(), 1f, Vector2.zero);
            }

            mousePositionNoZoom = Event.current.mousePosition;

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

            if (zoomer != null)
            {
                zoomer.Begin();

                for (int i = viewComponents.Length - 1; i >= 0; i--)
                    if (!IsInSink(Event.current) || viewComponents[i] is ICanvasMouseEventSink)
                        viewComponents[i].OnGUI();

                zoomer.End();
            }

            for (int i = c.Length - 1; i >= 0; i--)
                if (!IsInSink(Event.current) || c[i] is ICanvasMouseEventSink)
                    c[i].OnGUI();
        }

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

                        Vector2 mouse = component is IViewElement
                            ? mouseEvent.mousePosition * zoomer.zoom
                            : mouseEvent.mousePosition;

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

                    if (hovered == null && selectionBoxComponent != null)
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
            if (selectionBoxComponent != null && !selectionBoxComponent.hidden)
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

                Rect rect = component is IViewElement
                    ? new Rect(boxRect.position * zoomer.zoom, boxRect.size * zoomer.zoom)
                    : boxRect;

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
            GenericMenu menu = new();

            menu.AddItem("Copy", false, DoCopy, false);
            menu.AddItem("Paste", false, () => { }, true);

            menu.AddSeparator("");

            menu.AddItem("Delete", false, DeleteSelected, false);
            menu.AddItem("Duplicate", false, () => { }, false);

            menu.AddSeparator("");

            foreach (Type t in HelperMethods.GetEnumerableOfType(typeof(Node)))
            {
                string category = GraphObject.GetCategory(t);

                if (string.IsNullOrEmpty(category))
                    menu.AddItem($"Add Node/{t.GetFriendlyTypeName()}", false, () => { }, false);
                else
                    menu.AddItem($"Add Node/{category}/{t.GetFriendlyTypeName()}", false, () => { }, false);
            }

            foreach (Type t in HelperMethods.GetEnumerableOfType(typeof(Conditional)))
            {
                string category = GraphObject.GetCategory(t);

                if (string.IsNullOrEmpty(category))
                    menu.AddItem($"Add Conditional/{t.GetFriendlyTypeName()}", false, () => { }, false);
                else
                    menu.AddItem($"Add Conditional/{category}/{t.GetFriendlyTypeName()}", false, () => { }, false);
            }

            return menu;
        }
    }
}