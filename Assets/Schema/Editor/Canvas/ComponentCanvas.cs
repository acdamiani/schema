using System;
using System.Collections.Generic;
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
        public ComponentCanvas(SelectionBoxComponent.SelectionBoxComponentCreateArgs selectionBoxComponentCreateArgs)
        {
            selectionBoxComponentCreateArgs.layer = 1;
            _selectionBoxComponent = Create<SelectionBoxComponent>(selectionBoxComponentCreateArgs);
            _selectionBoxComponent.hidden = true;
        }
        public T Create<T>(CreateArgs args) where T : GUIComponent, new()
        {
            T component = new T();
            component.Create(args);
            component.layer = args.layer;
            ((GUIComponent)component).canvas = this;

            ArrayUtility.Add(ref _components, component);

            return component;
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
        public void Draw()
        {
            DoEvents();

            GUIComponent[] c = (GUIComponent[])components.Clone();
            Array.Sort(c, (a, b) => b.layer - a.layer);

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
        private void DoEvents()
        {
            switch (Event.current.type)
            {
                case EventType.MouseDown:
                    OnMouseDown(Event.current);
                    break;
                case EventType.MouseDrag:
                    OnMouseDrag(Event.current);
                    break;
                case EventType.ScrollWheel:
                    OnMouseScroll(Event.current);
                    break;
            }
        }
        private void OnMouseDown(Event mouseEvent)
        {
            switch (mouseEvent.button)
            {
                case 0:
                    for (int i = components.Length - 1; i >= 0; i--)
                    {
                        GUIComponent component = components[i];
                        ISelectable selectableComponent = component as ISelectable;

                        if (
                            selectableComponent == null
                                || !selectableComponent.IsSelectable()
                                || !selectableComponent.IsHit(mouseEvent.mousePosition)
                            )
                            continue;

                        components.MoveItemAtIndexToFront(i);

                        if (!mouseEvent.shift && !selected.Contains(component))
                            DeselectAll();

                        ArrayUtility.Add(ref _selected, component);

                        selectableComponent.Select(mouseEvent.shift);

                        return;
                    }

                    if (hovered == null)
                    {
                        selectionBoxComponent.mouseDownPosition = NodeEditor.WindowToGridPosition(mouseEvent.mousePosition);
                        selectionBoxComponent.hidden = false;
                    }

                    DeselectAll();

                    break;
            }
        }
        private void OnMouseDrag(Event mouseEvent)
        {
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