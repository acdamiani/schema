using UnityEditor;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Schema.Utilities;
using Schema;
using SchemaEditor.CustomEditors;

//Handles all events for the NodeEditor window
namespace SchemaEditor
{
    public partial class NodeEditor
    {
        private int controlId;
        private void ProcessEvents(Event e)
        {
            //Used to receive mouseUp events outside the window
            controlId = GUIUtility.GetControlID(FocusType.Passive);

            windowInfo.shouldCheckConnectionHover = false;

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
                        string[] possibleCommands = { "SoftDelete", "Delete", "FrameSelected", "Copy", "Paste", "SelectAll", "DeselectAll", "Duplicate" };

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
                                    List<Vector2> positions = windowInfo.selected.Select(node => node.graphPosition).ToList();

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
                                    PanView(-(target.root.graphPosition + GetAreaWithPadding(target.root, false) / 2f), 1f);
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
                            case "Duplicate":
                                e.Use();

                                Duplicate();
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
            windowInfo.resizingInspector = false;

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
                            // MoveDecorator(windowInfo.selectedDecorator, windowInfo.hoveredNode);
                        }
                    }
                    if (windowInfo.lastClicked == Window.Hovering.InConnection && requestingConnection != null && orphanNode != null && !editingPaused)
                    {
                        requestingConnection.RemoveConnection(orphanNode);
                        orphanNode = null;
                    }
                    if (windowInfo.lastClicked == Window.Hovering.Node && windowInfo.hoveredConnection != null && windowInfo.selected.Count == 1 && !editingPaused)
                    {
                        windowInfo.hoveredConnection.parent.SplitConnection(windowInfo.selected[0], windowInfo.hoveredConnection);

                        GetViewRect(100f, false);
                        target.TraverseTree();
                    }
                    break;
                case Window.Hovering.InConnection:
                    if (!editingPaused)
                    {
                        bool notChild;

                        if (requestingConnection == null) break;
                        else notChild = !requestingConnection.GetAllParents().Contains(windowInfo.hoveredNode);

                        if (orphanNode != null) requestingConnection.RemoveConnection(orphanNode);

                        if (notChild)
                        {
                            requestingConnection.AddConnection(windowInfo.hoveredNode);

                            requestingConnection.VerifyOrder();
                            target.TraverseTree();
                            requestingConnection = null;
                        }

                        orphanNode = null;
                    }
                    break;
                case Window.Hovering.Window:
                    if (windowInfo.lastClicked == Window.Hovering.InConnection && requestingConnection != null && orphanNode != null && !editingPaused)
                    {
                        requestingConnection.RemoveConnection(orphanNode);
                        orphanNode = null;
                        target.TraverseTree();
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

                    if (!drawBox && requestingConnection == null && inNodeEditor && windowInfo.lastClicked == Window.Hovering.Node && !editingPaused)
                    {
                        windowInfo.shouldCheckConnectionHover = true;

                        foreach (Node node in windowInfo.selected)
                        {
                            Vector2 rawMove = current.delta * windowInfo.zoom;
                            node.graphPosition += rawMove;

                            if (node.parent == null) continue;

                            int lastOrder = Array.IndexOf(node.parent.children, node);
                            node.parent.VerifyOrder();

                            //recalculate tree priorities if the order of this node changed
                            if (Array.IndexOf(node.parent.children, node) != lastOrder)
                                target.TraverseTree();
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
        private float GridSnap(float number, float size)
        {
            return Mathf.Round(number / size) * size;
        }

        private void MouseDown()
        {
            Event current = Event.current;

            // if (current.button == 0 && windowInfo.searchIsShown && !windowInfo.searchRect.Contains(current.mousePosition))
            // {
            //     ToggleSearch();
            // }
            if (windowInfo.searchIsShown)
            {
                return;
            }


            switch (current.button)
            {
                case 2:
                    requestingConnection = null;
                    windowInfo.isPanning = true;
                    break;
                case 1:
                    {
                        Rect r = new Rect(0f, 0f, position.width - windowInfo.inspectorWidth - GUIData.sidebarPadding * 2, position.height);
                        drawBox = false;
                        break;
                    }
                case 0 when !windowInfo.didDragSinceMouseUp:
                    {
                        if (windowInfo.hoveredType != Window.Hovering.Inspector && blackboardEditor)
                        {
                            ((BlackboardEditor)blackboardEditor).DeselectAll();
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
                                }

                                break;
                            case Window.Hovering.OutConnection:
                                if (windowInfo.hoveredNode.CanHaveChildren() && !editingPaused)
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
                case KeyCode.Escape:
                    windowInfo.searchIsShown = false;
                    break;
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
                        int index = Array.IndexOf(windowInfo.selectedDecorator.node.decorators, windowInfo.selectedDecorator);

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
                        int index = Array.IndexOf(windowInfo.selectedDecorator.node.decorators, windowInfo.selectedDecorator);

                        if (index + 1 > windowInfo.selectedDecorator.node.decorators.Length - 1)
                            break;

                        Select(windowInfo.selectedDecorator.node.decorators[index + 1]);
                    }
                    else if (windowInfo.selected.Count > 0 && windowInfo.selected[windowInfo.selected.Count - 1].children.Length > 0)
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
            if (windowInfo.searchIsShown)
            {
                return;
            }

            Event current = Event.current;

            if (windowInfo.hoveredType != Window.Hovering.Inspector && windowInfo.hoveredType != Window.Hovering.None)
                windowInfo.zoom += current.delta.y * GUIData.zoomSpeed;
            //windowInfo.zoom += (current.delta.y > 0 ? 1 : (current.delta.y < 0 ? -1 : 0)) * GUIData.zoomSpeed;
        }
    }
}