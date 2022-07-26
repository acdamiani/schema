using Schema;
using Schema.Utilities;
using SchemaEditor;
using SchemaEditor.Utilities;
using SchemaEditor.Internal;
using SchemaEditor.Internal.ComponentSystem;
using System;
using System.Text;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public sealed class NodeComponent : GUIComponent, ISelectable, IEditable, IFramable, IDeletable, IGraphObjectProvider, IViewElement
{
    private static readonly RectOffset ContentPadding = new RectOffset(20, 20, 14, 14);
    public Node node { get; set; }
    public string uID { get { return _uID; } }
    private string _uID;
    public NodeComponentLayout layout { get; set; }
    public ConnectionComponent parentConnection { get; set; }
    public static ConnectionComponent floatingConnection { get; set; }
    private Nullable<Vector2> beginDragPosition;
    private Vector2 beginDragNodePosition;
    private bool isSelected;
    public float xMin => layout.body.x;
    public float yMin => layout.body.y;
    public float xMax => layout.body.xMax;
    public float yMax => layout.body.yMax;
    public override void Create(CreateArgs args)
    {
        NodeComponentCreateArgs createArgs = args as NodeComponentCreateArgs;

        if (createArgs == null)
            throw new ArgumentException();

        if (createArgs.fromExisting != null)
        {
            node = createArgs.fromExisting;
            _uID = node.uID;

            if (node.parent != null)
            {
                ConnectionComponent.ConnectionComponentCreateArgs connectionComponentCreateArgs = new ConnectionComponent.ConnectionComponentCreateArgs();

                GUIComponent parentComponent = canvas.FindComponent(node.parent);

                if (parentComponent == null)
                {
                    Debug.LogWarning($"Component for node {node} has no active parent component! When creating NodeCompnents from existing nodes, do so in PreOrder.");
                }
                else
                {
                    connectionComponentCreateArgs.from = (NodeComponent)parentComponent;
                    connectionComponentCreateArgs.to = this;

                    parentConnection = canvas.Create<ConnectionComponent>(connectionComponentCreateArgs);
                }
            }
        }
        else
        {
            node = createArgs.graph.AddNode(createArgs.nodeType, createArgs.position);
        }

        layout = new NodeComponentLayout(this);
    }
    public override void OnGUI()
    {
        if (node == null)
            return;

        layout.Update();

        DoEvents();

        Color guiColor = GUI.color;

        GUI.color = Styles.windowBackground;

        if (
            node.connectionDescriptor == Node.ConnectionDescriptor.OnlyInConnection
            || node.connectionDescriptor == Node.ConnectionDescriptor.Both
        )
            GUI.DrawTexture(layout.inConnection, Styles.circle);

        if (
            node.connectionDescriptor == Node.ConnectionDescriptor.OnlyOutConnection
            || node.connectionDescriptor == Node.ConnectionDescriptor.Both
        )
            Styles.roundedBox.DrawIfRepaint(layout.outConnection, false, false, false, false);

        GUI.color = Styles.windowBackground;
        Styles.shadow.DrawIfRepaint(layout.shadow, false, false, false, false);

        GUI.color = isSelected ? Color.white : Styles.outlineColor;
        Styles.styles.nodeSelected.DrawIfRepaint(layout.body, false, false, false, false);

        GUI.color = Styles.windowAccent;
        Styles.roundedBox.DrawIfRepaint(layout.content, false, false, false, false);

        GUI.color = Color.white;

        Vector2 mousePos = Event.current.mousePosition;

        GUILayout.BeginArea(layout.content);
        GUILayout.Space(ContentPadding.top);

        GUILayout.Label(new GUIContent(node.name, node.icon), Styles.nodeLabel);

        GUILayout.Space(ContentPadding.bottom);
        GUILayout.EndArea();

        List<Error> errors = Enumerable.Empty<Error>().ToList();

        if (errors.Count > 0)
        {
            string errorTooltip = String.Join("\n", errors.Select(error => error.message));

            GUI.color = Styles.outlineColor;
            Styles.roundedBox.DrawIfRepaint(layout.errorBox, false, false, false, false);

            Rect tex = layout.errorBox.Pad(4);
            tex.position += new Vector2(1, 0);

            GUI.color = Color.white;
            GUI.Label(tex, new GUIContent(Styles.warnIcon, errorTooltip), GUIStyle.none);
        }

        if (node.priority > 0)
        {
            GUI.backgroundColor = Styles.outlineColor;
            Styles.roundedBox.DrawIfRepaint(layout.priorityIndicator, new GUIContent(node.priority.ToString()), false, false, false, false);
            GUI.backgroundColor = Color.white;
        }

        GUI.color = guiColor;
    }
    private void DoEvents()
    {
        Event e = Event.current;
        Vector2 mousePositionGrid = canvas.zoomer.WindowToGridPosition(canvas.mousePositionNoZoom);

        switch (e.rawType)
        {
            case EventType.MouseDown when e.button == 0:
                if (layout.inConnectionSliced.Contains(e.mousePosition))
                {
                    if (node.CanHaveParent())
                    {
                        ConnectionComponent.ConnectionComponentCreateArgs createArgs = new ConnectionComponent.ConnectionComponentCreateArgs();
                        createArgs.to = this;

                        floatingConnection = canvas.Create<ConnectionComponent>(createArgs);
                    }
                    else if (parentConnection != null)
                    {
                        parentConnection.to = null;
                        floatingConnection = parentConnection;
                    }
                }
                else if (layout.outConnectionSliced.Contains(e.mousePosition) && node.CanHaveChildren())
                {
                    ConnectionComponent.ConnectionComponentCreateArgs createArgs = new ConnectionComponent.ConnectionComponentCreateArgs();
                    createArgs.from = this;

                    floatingConnection = canvas.Create<ConnectionComponent>(createArgs);
                }
                break;
            case EventType.MouseUp:
                beginDragPosition = null;

                if (floatingConnection == null)
                    break;

                NodeComponent hovered = canvas.hovered as NodeComponent;

                if (hovered != null && hovered != this)
                {
                    HoverType hoverType = hovered.GetHoverType(Event.current.mousePosition);

                    if (hoverType == HoverType.InConnection && floatingConnection.to == null)
                        floatingConnection.to = hovered;
                    else if (hoverType == HoverType.OutConnection && floatingConnection.from == null && hovered.node.CanHaveChildren())
                        floatingConnection.from = hovered;

                    floatingConnection = null;
                }
                else
                {
                    Destroy(floatingConnection);
                    floatingConnection = null;
                }

                e.Use();
                break;
            case EventType.ScrollWheel when floatingConnection != null:
                e.Use();
                break;
            case EventType.MouseDrag when e.button == 0 && isSelected && canvas.selectionBoxComponent.hidden:
                float snap = Styles.gridTexture.width / 4f;

                if (beginDragPosition == null)
                {
                    beginDragPosition = mousePositionGrid;

                    if (NodeEditor.Prefs.gridSnap)
                    {
                        node.graphPosition = new Vector2(
                            Mathf.Round(node.graphPosition.x / snap) * snap,
                            Mathf.Round(node.graphPosition.y / snap) * snap
                        );
                    }

                    beginDragNodePosition = node.graphPosition;
                }

                Vector2 dxdy = mousePositionGrid - beginDragPosition.Value;

                if (NodeEditor.Prefs.gridSnap)
                {
                    dxdy = new Vector2(
                        Mathf.Round(dxdy.x / snap) * snap,
                        Mathf.Round(dxdy.y / snap) * snap
                    );
                }

                node.graphPosition = beginDragNodePosition + dxdy;

                break;
        }
    }
    public HoverType GetHoverType(Vector2 mousePosition)
    {
        if (layout.body.Contains(mousePosition))
            return HoverType.Body;
        else if (layout.inConnectionSliced.Contains(mousePosition))
            return HoverType.InConnection;
        else if (layout.outConnectionSliced.Contains(mousePosition))
            return HoverType.OutConnection;

        return HoverType.None;
    }
    public override bool ShouldHover(Vector2 mousePosition)
    {
        return layout.body.Contains(mousePosition)
            || layout.inConnection.Contains(mousePosition)
            || layout.outConnection.Contains(mousePosition);
    }
    public override bool ResolveObject(UnityEngine.Object obj) { return obj == node; }
    public override string GetDebugInfo()
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine(String.Format("<b>Name:</b> {0}", node.name));
        sb.AppendLine(String.Format("<b>GUID:</b> {0}", node.uID));
        sb.AppendLine(String.Format("<b>Status Indicator Enabled:</b> {0}", node.enableStatusIndicator));
        sb.AppendLine(String.Format("<b>Connection Descriptor:</b> {0}", node.connectionDescriptor.ToString()));
        sb.AppendLine(String.Format("<b>Graph Position:</b> {0}", node.graphPosition));
        sb.AppendLine();

        sb.AppendLine(String.Format("<b>Parent:</b> {0}", node.parent?.name));
        sb.Append("<b>Children:</b> ");
        sb.AppendLine(String.Join(", ", node.children.Select(x => x.name)));
        sb.Append("<b>Conditionals:</b> ");
        sb.AppendLine(String.Join(", ", node.conditionals.Select(x => x.name)));
        sb.Append("<b>Modifiers:</b> ");
        sb.AppendLine(String.Join(", ", node.modifiers.Select(x => x.name)));
        sb.AppendLine();

        sb.AppendLine(String.Format("<b>Body:</b> {0}", layout.body));
        sb.AppendLine(String.Format("<b>Content:</b> {0}", layout.content));
        sb.AppendLine(String.Format("<b>Shadow:</b> {0}", layout.shadow));
        sb.AppendLine(String.Format("<b>InConnection:</b> {0}", layout.inConnection));
        sb.AppendLine(String.Format("<b>OutConnection:</b> {0}", layout.outConnection));
        sb.AppendLine(String.Format("<b>InConnectionSliced:</b> {0}", layout.inConnectionSliced));
        sb.AppendLine(String.Format("<b>OutConnectionSliced:</b> {0}", layout.outConnectionSliced));
        sb.AppendLine(String.Format("<b>ErrorBox:</b> {0}", layout.errorBox));
        sb.Append(String.Format("<b>PriorityIndicator:</b> {0}", layout.priorityIndicator));

        return sb.ToString();
    }
    public bool IsSelectable() { return true; }
    public bool IsHit(Vector2 mousePosition) { return layout.gridRect.Contains(canvas.zoomer.WindowToGridPosition(mousePosition)); }
    public bool Overlaps(Rect rect) { return layout.body.Overlaps(rect, true); }
    public void Select(bool additive) { isSelected = true; }
    public void Deselect() { isSelected = false; }
    public UnityEngine.Object GetEditable() { return node; }
    public bool IsEditable() { return true; }
    public bool IsFramable() { return true; }
    public bool IsDeletable() { return isSelected; }
    public void Delete() { node.graph.DeleteNodes(new List<Node>() { node }); }
    public bool Equals(Schema.Internal.GraphObject graphObject)
    {
        Node node = graphObject as Node;

        if (node != null)
            return node.uID == uID;
        else
            return false;
    }
    public class NodeComponentCreateArgs : CreateArgs
    {
        public Graph graph { get; set; }
        public Node fromExisting { get; set; }
        public Type nodeType { get; set; }
        public Vector2 position { get; set; }
    }
    public enum HoverType
    {
        None,
        Body,
        InConnection,
        OutConnection
    }
    public class NodeComponentLayout
    {
        private struct ShallowNode
        {
            public ShallowNode(Node node)
            {
                position = node.graphPosition;
                name = node.name;
                priority = node.priority;
            }
            public Vector2 position { get; set; }
            public string name { get; set; }
            public int priority { get; set; }
            public override bool Equals(object other)
            {
                if (!(other is ShallowNode))
                    return false;

                ShallowNode otherNode = (ShallowNode)other;

                return position == otherNode.position && name == otherNode.name && priority == otherNode.priority;
            }
            public override int GetHashCode()
            {
                return position.GetHashCode() + name.GetHashCode() + priority;
            }
        }
        public NodeComponentLayout(NodeComponent component)
        {
            this.component = component;
        }
        public void Update()
        {
            ShallowNode current = new ShallowNode(component.node);

            if (!current.Equals(last))
                DoRect();

            last = current;

            Rect r = gridRect;
            r.position = component.canvas.zoomer.GridToWindowPositionNoClipped(r.position);

            Vector2 v = Styles.roundedBox.CalcSize(new GUIContent(last.priority.ToString()));
            v = new Vector2(Mathf.Max(24, v.x + 10), Mathf.Max(24, v.y + 10));

            body = r;
            shadow = body.Pad(-14);
            content = body.Pad(14);
            errorBox = new Rect(body.xMax, body.yMax, 40f, 40f).UseCenter();
            inConnection = new Rect(body.center.x, body.y, 24f, 24f).UseCenter();
            outConnection = new Rect(body.center.x, body.yMax, body.width - 48f, 24f).UseCenter();
            inConnectionSliced = inConnection.Slice(0.5f, false, true);
            outConnectionSliced = outConnection.Slice(0.5f, false, false);
            priorityIndicator = new Rect(body.x, body.center.y, v.x, v.y).UseCenter();
        }
        private void DoRect()
        {
            Vector2 labelSize = Styles.nodeLabel.CalcSize(new GUIContent(component.node.name, component.node.icon));

            float width = labelSize.x + ContentPadding.left + ContentPadding.right + 28;
            float height = labelSize.y + ContentPadding.top + ContentPadding.bottom + 28;

            gridRect = new Rect(component.node.graphPosition, new Vector2(width, height));
        }
        private NodeComponent component { get; set; }
        private ShallowNode last { get; set; }
        public Rect gridRect { get; set; }
        public Rect body { get; set; }
        public Rect content { get; set; }
        public Rect shadow { get; set; }
        public Rect inConnection { get; set; }
        public Rect outConnection { get; set; }
        public Rect inConnectionSliced { get; set; }
        public Rect outConnectionSliced { get; set; }
        public Rect errorBox { get; set; }
        public Rect priorityIndicator { get; set; }
    }
}