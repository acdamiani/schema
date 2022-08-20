using Schema;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly List<Func<GUIComponent, bool>> selectors = new();

        public SchemaAgent activeInScene { get { return _activeInScene; } }
        private SchemaAgent _activeInScene;

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

            onComponentListModified?.Invoke();

            MoveToFront(component);

            int i = selectors.FindIndex(x => x(component));

            if (i >= 0)
            {
                selectors.RemoveAt(i);
                Select(component);
            }

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
            if (ArrayUtility.Contains(_components, component))
                ArrayUtility.Remove(ref _components, component);

            onComponentListModified?.Invoke();
        }

        public void Select(GUIComponent component)
        {
            ISelectable selectable = component as ISelectable;

            if (selectable == null || !selectable.IsSelectable())
                return;

            selectable.Select(true);

            if (!ArrayUtility.Contains(_selected, component))
                ArrayUtility.Add(ref _selected, component);
        }

        public void Deselect(GUIComponent component)
        {
            ISelectable selectable = component as ISelectable;

            if (selectable == null)
                return;

            selectable.Deselect();

            if (ArrayUtility.Contains(_selected, component))
                ArrayUtility.Remove(ref _selected, component);
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

            SchemaAgent current = Selection.gameObjects
                .Count() != 1 ? null : Selection.gameObjects.First().GetComponent<SchemaAgent>();

            if (current != null)
                _activeInScene = current;

            if (!IsInSink(Event.current))
                EditorGUIUtility.AddCursorRect(context.GetViewRect(), cursor);

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
                (a, b) => b.layer - a.layer
            );

            DoEvents(c);

            c = c.Intersect(components).ToArray();

            GUIComponent[] viewComponents = c.Where(c => c is IViewElement).ToArray();
            c = c.Except(viewComponents).ToArray();

            bool isSinkEvent = IsSinkEvent(Event.current);

            if (zoomer != null)
            {
                zoomer.Begin();

                for (int i = viewComponents.Length - 1; i >= 0; i--)
                    if (!isSinkEvent || viewComponents[i] is ICanvasMouseEventSink)
                        viewComponents[i].OnGUI();

                zoomer.End();
            }

            for (int i = c.Length - 1; i >= 0; i--)
                if (!isSinkEvent || c[i] is ICanvasMouseEventSink)
                    c[i].OnGUI();
        }

        public void DeselectAll()
        {
            foreach (GUIComponent component in selected)
                ((ISelectable)component).Deselect();

            ArrayUtility.Clear(ref _selected);
        }

        public void SelectWhenCreated(Func<GUIComponent, bool> selector)
        {
            selectors.Add(selector);
        }

        public bool CanSelectChildren()
        {
            IEnumerable<GUIComponent> nodeComponents = selected
                .Where(x => x is NodeComponent);

            return nodeComponents.Count() > 0;
        }

        private void DoEvents(IEnumerable<GUIComponent> layeredComponents)
        {
            if (IsSinkEvent(Event.current))
                return;

            switch (Event.current.rawType)
            {
                case EventType.ValidateCommand:
                    if (CommandHandler.Valdiate(Event.current.commandName))
                        Event.current.Use();
                    break;
                case EventType.ExecuteCommand:
                    CommandHandler.Execute(this, Event.current.commandName);
                    break;
                case EventType.MouseDown:
                    OnMouseDown(Event.current, layeredComponents);
                    break;
                case EventType.MouseUp:
                    OnMouseUp(Event.current);
                    break;
                case EventType.MouseDrag:
                    OnMouseDrag(Event.current);
                    break;
                case EventType.ScrollWheel:
                    OnMouseScroll(Event.current);
                    break;
            }
        }

        private bool IsSinkEvent(Event sinkEvent)
        {
            if (!sinkEvent.isKey && !sinkEvent.isMouse && !sinkEvent.isScrollWheel)
                return false;

            return IsInSink(sinkEvent);
        }

        private bool IsInSink(Event mouseEvent)
        {
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

        private void OnMouseUp(Event mouseEvent)
        {
            switch (mouseEvent.button)
            {
                case 0:
                    GUIUtility.hotControl = 0;
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
            if (zoomer != null)
                zoomer.zoom += mouseEvent.delta.y * Prefs.zoomSpeed;
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
            ContextBuilder menu = new();

            menu.AddShortcut("Main Menu/Edit/Cut", () => CommandHandler.CutCommand(this));
            menu.AddShortcut("Main Menu/Edit/Copy", () => CommandHandler.CopyCommand(this));
            menu.AddShortcut("Main Menu/Edit/Paste", () => CommandHandler.PasteCommand(this));

            menu.AddSeparator();

            menu.AddShortcut("Main Menu/Edit/Duplicate", () => CommandHandler.DuplicateCommand(this));
            menu.AddShortcut("Main Menu/Edit/Delete", () => CommandHandler.DeleteCommand(this));

            menu.AddSeparator();

            menu.AddShortcut("Main Menu/Edit/Undo", Undo.PerformUndo);
            menu.AddShortcut("Main Menu/Edit/Redo", Undo.PerformRedo);

            menu.AddSeparator();

            menu.AddShortcut("Main Menu/Edit/Select All", () => CommandHandler.SelectAllCommand(this));
            menu.AddShortcut("Main Menu/Edit/Deselect All", () => CommandHandler.DeselectAllCommand(this));
            menu.AddShortcut("Main Menu/Edit/Select Children", () => CommandHandler.SelectChildrenCommand(this),
                !CanSelectChildren());

            menu.AddSeparator();

            menu.AddShortcut("Schema/Add Node", NodeEditor.AddNodeCommand);
            menu.AddShortcut("Schema/Add Conditional", NodeEditor.AddConditionalCommand,
                NodeEditor.CanAddConditional(this));
            menu.AddShortcut("Schema/Break Connections", NodeEditor.BreakConnectionsCommand);

            return menu.menu;
        }
    }
}