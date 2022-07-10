using Schema;
using Schema.Utilities;
using SchemaEditor;
using SchemaEditor.Utilities;
using SchemaEditor.Internal;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public sealed class NodeComponent : GUIComponent
{
    private static readonly RectOffset ContentPadding = new RectOffset(20, 20, 14, 14);
    public Node node { get; set; }
    public NodeComponentLayout layout { get; set; }
    public static ConnectionComponent floatingConnection;
    public override void Create(CreateArgs args)
    {
        NodeComponentCreateArgs createArgs = args as NodeComponentCreateArgs;

        if (createArgs == null)
            throw new ArgumentException();

        if (createArgs.fromExisting != null)
            node = createArgs.fromExisting;
        else
            node = createArgs.graph.AddNode(createArgs.nodeType, createArgs.position);

        layout = new NodeComponentLayout(this);
    }
    public override void OnGUI()
    {
        ConditionalComponent c = new ConditionalComponent();
        c.Do(layout.body.position);

        layout.Update();

        DoEvents();

        Color guiColor = GUI.color;

        GUI.color = Styles.windowBackground;

        if (node.CanHaveParent())
            GUI.DrawTexture(layout.inConnection, Styles.circle);

        if (node.CanHaveChildren())
            Styles.roundedBox.DrawIfRepaint(layout.outConnection, false, false, false, false);

        GUI.color = Styles.windowBackground;
        Styles.shadow.DrawIfRepaint(layout.shadow, false, false, false, false);

        GUI.color = Styles.outlineColor;
        Styles.styles.nodeSelected.DrawIfRepaint(layout.body, false, false, false, false);

        GUI.color = Styles.windowAccent;
        Styles.roundedBox.DrawIfRepaint(layout.content, false, false, false, false);

        GUI.color = Color.white;

        GUILayout.BeginArea(layout.content);
        GUILayout.Space(ContentPadding.top);

        GUILayout.Label(new GUIContent(node.name, node.icon), Styles.nodeLabel);

        GUILayout.Space(ContentPadding.bottom);
        GUILayout.EndArea();

        List<Error> errors = node.GetErrors();

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

        switch (e.type)
        {
            case EventType.MouseDown when e.button == 0:
                if (layout.inConnection.Contains(e.mousePosition))
                {
                    ConnectionComponent.ConnectionComponentCreateArgs createArgs = new ConnectionComponent.ConnectionComponentCreateArgs();
                    createArgs.to = this;

                    floatingConnection = NodeEditor.instance.canvas.Create<ConnectionComponent>(createArgs);
                    e.Use();
                }
                else if (layout.outConnection.Contains(e.mousePosition))
                {
                    ConnectionComponent.ConnectionComponentCreateArgs createArgs = new ConnectionComponent.ConnectionComponentCreateArgs();
                    createArgs.from = this;

                    floatingConnection = NodeEditor.instance.canvas.Create<ConnectionComponent>(createArgs);
                    e.Use();
                }
                break;
            case EventType.MouseUp when floatingConnection != null:
                NodeEditor.instance.canvas.Remove(floatingConnection);
                floatingConnection = null;
                e.Use();
                break;
            case EventType.ScrollWheel when floatingConnection != null:
                e.Use();
                break;
        }
    }
    public HoverType GetHoverType(Vector2 mousePosition)
    {
        if (layout.body.Contains(mousePosition))
            return HoverType.Body;
        else if (layout.inConnection.Contains(mousePosition))
            return HoverType.InConnection;
        else if (layout.outConnection.Contains(mousePosition))
            return HoverType.OutConnection;

        return HoverType.None;
    }
    public override bool IsHovered(Vector2 mousePosition)
    {
        return layout.body.Contains(mousePosition)
            || layout.inConnection.Contains(mousePosition)
            || layout.outConnection.Contains(mousePosition);
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
            r.position = NodeEditor.GridToWindowPositionNoClipped(r.position);

            Vector2 v = Styles.roundedBox.CalcSize(new GUIContent(last.priority.ToString()));
            v = new Vector2(Mathf.Max(24, v.x + 10), Mathf.Max(24, v.y + 10));

            body = r;
            shadow = body.Pad(-14);
            content = body.Pad(14);
            errorBox = new Rect(body.xMax, body.yMax, 40f, 40f).UseCenter();
            inConnection = new Rect(body.center.x, body.y, 24f, 24f).UseCenter();
            outConnection = new Rect(body.center.x, body.yMax, body.width - 48f, 24f).UseCenter();
            priorityIndicator = new Rect(body.x, body.center.y, v.x, v.y).UseCenter();
        }
        private void DoRect()
        {
            Vector2 labelSize = Styles.nodeLabel.CalcSize(new GUIContent(component.node.name, component.node.icon));

            float width = labelSize.x + ContentPadding.left + ContentPadding.right + 28;
            float height = labelSize.y + ContentPadding.top + ContentPadding.bottom + 28;

            gridRect = new Rect(component.node.graphPosition - new Vector2(250f, 0f), new Vector2(width, height)).UseCenter();
        }
        private NodeComponent component { get; set; }
        private ShallowNode last { get; set; }
        private Rect gridRect { get; set; }
        public Rect body { get; set; }
        public Rect content { get; set; }
        public Rect shadow { get; set; }
        public Rect inConnection { get; set; }
        public Rect outConnection { get; set; }
        public Rect errorBox { get; set; }
        public Rect priorityIndicator { get; set; }
    }
}