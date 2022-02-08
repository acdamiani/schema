using UnityEditor;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Schema.Utilities;
using Schema.Runtime;

//Handles all events for the NodeEditor window
namespace Schema.Editor
{
    public partial class NodeEditor
    {
        private int controlId;
        private void ProcessEvents(Event e)
        {
            //Used to receive mouseUp events outside the window
            controlId = GUIUtility.GetControlID(FocusType.Passive);

            switch (e.rawType)
            {
                case EventType.MouseDown:
                    MouseDown();
                    break;
                case EventType.MouseUp:
                    MouseUp();
                    break;
                case EventType.MouseDrag:
                    MouseDrag();
                    break;
                case EventType.KeyDown:
                    KeyDown();
                    break;
                case EventType.ScrollWheel:
                    if (!drawBox && !windowInfo.didDragSinceMouseUp) ScrollWheel();
                    break;
                case EventType.ValidateCommand:
                    if (!editingPaused)
                    {
                        string[] possibleCommands = { "SoftDelete", "Delete", "FrameSelected", "Copy", "Paste", "SelectAll", "DeselectAll" };

                        if (possibleCommands.Contains(e.commandName))
                            e.Use();
                    }
                    break;
                case EventType.ExecuteCommand:
                    if (!editingPaused)
                    {
                        switch (e.commandName)
                        {
                            case "SoftDelete":
                                e.Use();

                                DeleteSelected();
                                break;
                            case "Delete":
                                e.Use();

                                DeleteSelected();
                                break;
                            case "FrameSelected":
                                e.Use();

                                if (windowInfo.selected.Count > 0)
                                {
                                    List<Vector2> positions = windowInfo.selected.Select(node => node.position).ToList();

                                    float x = 0f;
                                    float y = 0f;

                                    foreach (Vector2 pos in positions)
                                    {
                                        Vector2 area = GetAreaWithPadding(windowInfo.selected[positions.IndexOf(pos)], false);

                                        x += pos.x + area.x / 2f;
                                        y += pos.y + area.y / 2f;
                                    }

                                    Vector2 avg = new Vector2(x / windowInfo.selected.Count, y / windowInfo.selected.Count);

                                    PanView(-avg, 1f);
                                }
                                else
                                {
                                    PanView(-(target.root.position + GetAreaWithPadding(target.root, false) / 2f), 1f);
                                }
                                break;
                            case "Copy":
                                e.Use();

                                if (windowInfo.selectedDecorator != null)
                                    Copy(windowInfo.selectedDecorator);
                                else if (windowInfo.selected.Count > 0)
                                    Copy(windowInfo.selected);
                                break;
                            case "Paste":
                                e.Use();

                                Paste();
                                break;
                            case "SelectAll":
                                e.Use();

                                List<Node> iteration = new List<Node>(target.nodes);

                                foreach (Node node in iteration)
                                {
                                    Select(node, true);
                                }
                                break;
                            case "DeselectAll":
                                e.Use();

                                windowInfo.selected.Clear();
                                break;
                        }

                        Repaint();
                    }
                    break;
            }
        }
        private void MouseUp()
        {
            Event current = Event.current;

            windowInfo.isPanning = false;

            if (windowInfo.lastClicked == Window.Hovering.Decorator && windowInfo.didDragSinceMouseUp)
            {
                MoveDecoratorInNode(windowInfo.selectedDecorator, windowInfo.hoveredDecoratorIndex);
            }

            if (current.button == 1)
                GenerateContextMenu().ShowAsContext();

            switch (windowInfo.hoveredType)
            {
                case Window.Hovering.Node:
                    if (windowInfo.selectedDecorator != null && !drawBox && windowInfo.didDragSinceMouseUp && !current.shift && current.button == 0)
                    {
                        if (windowInfo.selectedDecorator.node != windowInfo.hoveredNode)
                        {
                            MoveDecorator(windowInfo.selectedDecorator, windowInfo.hoveredNode);
                        }
                    }
                    if (windowInfo.lastClicked == Window.Hovering.InConnection && requestingConnection != null && orphanNode != null && !editingPaused)
                    {
                        RemoveConnection(requestingConnection, orphanNode);
                        orphanNode = null;
                    }
                    break;
                case Window.Hovering.InConnection:
                    if (!editingPaused)
                    {
                        bool notChild;

                        if (requestingConnection == null) break;
                        else notChild = !requestingConnection.GetAllParents().Contains(windowInfo.hoveredNode);

                        if (orphanNode != null) RemoveConnection(requestingConnection, orphanNode);

                        if (notChild)
                        {
                            if (!windowInfo.hoveredNode.canHaveParent)
                            {
                                windowInfo.hoveredNode.parent.children.Remove(windowInfo.hoveredNode);
                            }
                            AddConnection(requestingConnection, windowInfo.hoveredNode);
                            RecalculatePriorities(windowInfo.hoveredNode.parent);
                            TraverseTree();
                            requestingConnection = null;
                        }

                        orphanNode = null;
                    }
                    break;
                case Window.Hovering.Window:
                    if (windowInfo.lastClicked == Window.Hovering.InConnection && requestingConnection != null && orphanNode != null && !editingPaused)
                    {
                        RemoveConnection(requestingConnection, orphanNode);
                        orphanNode = null;
                    }
                    break;
                case Window.Hovering.MinimapNode:
                    if (windowInfo.didDragSinceMouseUp) break;

                    if (windowInfo.selected.Contains(windowInfo.hoveredNode) && current.shift)
                    {
                        Deselect(windowInfo.hoveredNode);
                    }
                    else if (windowInfo.selected.Count < 2 || !windowInfo.selected.Contains(windowInfo.hoveredNode))
                    {
                        Select(windowInfo.hoveredNode, current.shift);
                    }
                    break;
            }

            if (windowInfo.movingDivider) windowInfo.movingDivider = false;

            requestingConnection = null;
            drawBox = false;
            windowInfo.didDragSinceMouseUp = false;
            windowInfo.lastClicked = Window.Hovering.None;
        }
        private void MouseDrag()
        {
            Event current = Event.current;

            windowInfo.didDragSinceMouseUp = true;
            bool inNodeEditor = windowInfo.hoveredType != Window.Hovering.Inspector && windowInfo.hoveredType != Window.Hovering.None;

            switch (current.button)
            {
                case 0:
                    if (windowInfo.hoveredType == Window.Hovering.Minimap || windowInfo.hoveredType == Window.Hovering.MinimapNode && !drawBox)
                    {
                        Vector2 ratio = window.size / (windowInfo.minimapView.size);

                        windowInfo.pan -= current.delta * ratio * windowInfo.zoom;
                        windowInfo.isPanning = true;
                        windowInfo.didDragSinceMouseUp = true;
                        break;
                    }

                    if (windowInfo.movingDivider)
                    {
                        windowInfo.dividerPos = current.mousePosition.y;
                    }
                    else if (!drawBox && requestingConnection == null && inNodeEditor && windowInfo.lastClicked == Window.Hovering.Node && !editingPaused)
                    {
                        foreach (Node node in windowInfo.selected)
                        {
                            node.position += current.delta * windowInfo.zoom;

                            if (node.parent == null) continue;

                            int lastOrder = node.parent.children.IndexOf(node);
                            RecalculatePriorities(node.parent);

                            //recalculate tree priorities if the order of this node changed
                            if (node.parent.children.IndexOf(node) != lastOrder)
                                TraverseTree();
                        }

                        GetViewRect(100f, true);
                    }
                    break;
                case 2:
                    drawBox = false;
                    if (windowInfo.hoveredType != Window.Hovering.Inspector && windowInfo.hoveredType != Window.Hovering.None)
                    {
                        windowInfo.isPanning = true;
                        windowInfo.pan += current.delta * windowInfo.zoom;
                    }
                    else
                        windowInfo.isPanning = false;
                    break;
            }
        }

        private void MouseDown()
        {
            Event current = Event.current;

            if (current.button == 0 && windowInfo.searchIsShown && !windowInfo.searchRect.Contains(current.mousePosition))
                ToggleSearch();

            switch (current.button)
            {
                case 2:
                    requestingConnection = null;
                    windowInfo.isPanning = true;
                    break;
                case 1:
                    {
                        Rect r = new Rect(0f, 0f, position.width - GUIData.inspectorWidth - GUIData.sidebarPadding * 2, position.height);
                        drawBox = false;
                        break;
                    }
                case 0 when windowInfo.hoveringDivider:
                    windowInfo.movingDivider = true;
                    break;
                case 0 when !windowInfo.didDragSinceMouseUp:
                    {
                        if (windowInfo.hoveredType != Window.Hovering.Inspector && blackboardEditor)
                        {
                            ((BlackboardEditor)blackboardEditor).selectedEntry = null;
                        }

                        if (windowInfo.hoveredType != Window.Hovering.Inspector) GUI.FocusControl(null);
                        switch (windowInfo.hoveredType)
                        {
                            case Window.Hovering.Window:
                                if (windowInfo.selectedDecorator == null)
                                    windowInfo.selected.Clear();
                                else windowInfo.selectedDecorator = null;

                                drawBox = true;
                                windowInfo.mouseDownPos = WindowToGridPosition(current.mousePosition);

                                SceneView.RepaintAll();

                                break;
                            case Window.Hovering.Node:
                                if (windowInfo.selected.Contains(windowInfo.hoveredNode) && current.shift)
                                {
                                    Deselect(windowInfo.hoveredNode);
                                }
                                else if (windowInfo.selected.Count < 2 || !windowInfo.selected.Contains(windowInfo.hoveredNode))
                                {
                                    Select(windowInfo.hoveredNode, current.shift);
                                    //Selection.activeObject = hovered;
                                    if (windowInfo.selectedDecorator != null)
                                    {
                                        Deselect(windowInfo.selectedDecorator);
                                    }
                                }

                                break;
                            case Window.Hovering.InConnection:
                                if (windowInfo.hoveredNode.parent != null && !editingPaused)
                                {
                                    requestingConnection = windowInfo.hoveredNode.parent;
                                    orphanNode = windowInfo.hoveredNode;
                                    TraverseTree();
                                }

                                break;
                            case Window.Hovering.OutConnection:
                                if (windowInfo.hoveredNode.canHaveChildren && !editingPaused)
                                {
                                    requestingConnection = windowInfo.hoveredNode;
                                }

                                break;
                            case Window.Hovering.Decorator:
                                Select(windowInfo.hoveredDecorator);
                                break;
                        }

                        windowInfo.lastClicked = windowInfo.hoveredType;
                        break;
                    }
            }

            if (current.button == 0)
            {
                //This allows our window to receive mouseUp events outside the window 
                //see https://answers.unity.com/questions/34694/detecting-mouseup-event-when-the-mouse-is-not-over.html
                //this HAS to be done only on left click because otherwise it causes issues with the GenericMenu
                //Menu still focused even after dismissed?
                GUIUtility.hotControl = controlId;
            }

            windowInfo.didDragSinceMouseUp = false;
        }
        private void KeyDown()
        {
            Event current = Event.current;

            Node next;

            switch (current.keyCode)
            {
                case KeyCode.H:
                case KeyCode.LeftArrow:
                    if (windowInfo.selected.Count == 0 || windowInfo.searchIsShown)
                        return;

                    next = GetSiblingNode(windowInfo.selected[windowInfo.selected.Count - 1], true);

                    if (next == null)
                        return;

                    Select(next, false);
                    break;
                case KeyCode.L:
                case KeyCode.RightArrow:
                    if (windowInfo.selected.Count == 0 || windowInfo.searchIsShown)
                        return;

                    next = GetSiblingNode(windowInfo.selected[windowInfo.selected.Count - 1], false);

                    if (next == null)
                        return;

                    Select(next, false);
                    break;
                case KeyCode.K:
                case KeyCode.UpArrow:
                    if (windowInfo.searchIsShown || current.control || current.shift || current.alt)
                        break;

                    if (windowInfo.selectedDecorator != null)
                    {
                        int index = windowInfo.selectedDecorator.node.decorators.IndexOf(windowInfo.selectedDecorator);

                        if (index - 1 < 0)
                            break;

                        Select(windowInfo.selectedDecorator.node.decorators[index - 1]);
                    }
                    else if (windowInfo.selected.Count > 0 && windowInfo.selected[windowInfo.selected.Count - 1].parent != null)
                    {
                        next = windowInfo.selected[windowInfo.selected.Count - 1].parent;

                        if (next == null)
                            break;

                        Select(next, false);
                    }

                    break;
                case KeyCode.J:
                case KeyCode.DownArrow:
                    if (windowInfo.searchIsShown || current.control || current.shift || current.alt)
                        break;

                    if (windowInfo.selectedDecorator != null)
                    {
                        int index = windowInfo.selectedDecorator.node.decorators.IndexOf(windowInfo.selectedDecorator);

                        if (index + 1 > windowInfo.selectedDecorator.node.decorators.Count - 1)
                            break;

                        Select(windowInfo.selectedDecorator.node.decorators[index + 1]);
                    }
                    else if (windowInfo.selected.Count > 0 && windowInfo.selected[windowInfo.selected.Count - 1].children.Count > 0)
                    {
                        next = windowInfo.selected[windowInfo.selected.Count - 1].children[0];

                        Select(next, false);
                    }

                    break;
            }

            ShortcutManager.KeyPress(current);
        }
        private void ScrollWheel()
        {
            Event current = Event.current;

            if (windowInfo.hoveredType != Window.Hovering.Inspector && windowInfo.hoveredType != Window.Hovering.None)
                windowInfo.zoom += current.delta.y * GUIData.zoomSpeed;
            //windowInfo.zoom += (current.delta.y > 0 ? 1 : (current.delta.y < 0 ? -1 : 0)) * GUIData.zoomSpeed;
        }
        void RegisterShortcuts()
        {
            ShortcutManager.AddShortcut(KeyCode.B, EventModifiers.Control, () =>
            {
                foreach (Node node in windowInfo.selected)
                {
                    if (node.parent)
                        RemoveConnection(node.parent, node);

                    List<Node> ch = new List<Node>(node.children);
                    foreach (Node c in ch)
                    {
                        RemoveConnection(node, c);
                    }
                }
            });
        }
        //Adds a builtin shortcut not registered as a command
        void AddCommandShortcut(string commandName, UnityEngine.Events.UnityAction action)
        {
            UnityEditor.ShortcutManagement.KeyCombination keyCombination = GetCommandKeyCombination(commandName);

            //Explicit cast is incorrect
            //EventModifiers e = (EventModifiers)keyCombination.modifiers;

            EventModifiers e = EventModifiers.None;

            if (keyCombination.action)
                e |= HelperMethods.IsMac() ? EventModifiers.Command : EventModifiers.Control;
            if (keyCombination.alt)
                e |= EventModifiers.Alt;
            if (keyCombination.shift)
                e |= EventModifiers.Shift;

            ShortcutManager.AddShortcut(keyCombination.keyCode, e, action);
        }
        void ResetShortcuts(UnityEditor.ShortcutManagement.ActiveProfileChangedEventArgs args)
        {
            ShortcutManager.ClearShortcuts();
            RegisterShortcuts();
        }
    }
}