using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Schema;
using Schema.Internal;
using Schema.Utilities;
using SchemaEditor.Utilities;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SchemaEditor.Internal.ComponentSystem.Components
{
    public sealed class NodeComponent
        : GUIComponent, ISelectable, IEditable, IFramable, IDeletable, IGraphObjectProvider, IViewElement, ICopyable, IPasteRecievier
    {
        public enum HoverType
        {
            None,
            Body,
            InConnection,
            OutConnection
        }

        private static readonly RectOffset ContentPadding = new(20, 20, 14, 14);
        private Vector2 beginDragNodePosition;
        private Vector2? beginDragPosition;
        private static bool beginConnectionOrigin;
        private float cLerp;
        private bool isActive;
        private bool isSelected;
        private NodeStatus status;
        private Color statusColor;
        private float t;
        public Node node { get; private set; }

        public string uID { get; private set; }

        public NodeComponentLayout layout { get; set; }
        public ConnectionComponent parentConnection { get; set; }
        public static ConnectionComponent floatingConnection { get; set; }

        public bool IsDeletable()
        {
            return isSelected;
        }

        public void Delete()
        {
            IEnumerable<ConditionalComponent> conditionalComponents = canvas.components
                .Where(x => x is ConditionalComponent)
                .Cast<ConditionalComponent>()
                .Where(x => x.conditional.node == node);

            foreach (ConditionalComponent conditional in conditionalComponents)
                Destroy(conditional);

            node.graph.DeleteNode(node);

            if (parentConnection != null)
                Destroy(parentConnection);
        }

        public Object GetEditable()
        {
            return node;
        }

        public bool IsEditable()
        {
            return true;
        }

        public float xMin => layout.body.x;
        public float yMin => layout.body.y;
        public float xMax => layout.body.xMax;
        public float yMax => layout.body.yMax;

        public bool IsFramable()
        {
            return true;
        }

        public bool Equals(GraphObject graphObject)
        {
            Node node = graphObject as Node;

            if (node != null)
                return node.uID == uID;

            return false;
        }

        public bool IsSelected()
        {
            return isSelected;
        }

        public bool IsSelectable()
        {
            return true;
        }

        public bool IsHit(Vector2 mousePosition)
        {
            return layout.body.Contains(mousePosition);
        }

        public bool Overlaps(Rect rect)
        {
            return layout.body.Overlaps(rect, true);
        }

        public void Select(bool additive)
        {
            isSelected = true;
        }

        public void Deselect()
        {
            isSelected = false;
        }

        public bool IsCopyable()
        {
            return isSelected;
        }

        public bool IsPastable()
        {
            return isSelected;
        }

        public Object GetCopyable()
        {
            return node;
        }

        public Object GetReciever()
        {
            return node;
        }

        public override void Create(CreateArgs args)
        {
            NodeComponentCreateArgs createArgs = args as NodeComponentCreateArgs;

            if (createArgs == null)
                throw new ArgumentException();

            createArgs.layer = 2;

            if (createArgs.fromExisting != null)
            {
                node = createArgs.fromExisting;
                uID = node.uID;
            }
            else
            {
                node = createArgs.graph.AddNode(createArgs.nodeType, createArgs.position);
                uID = node.uID;
            }

            layout = new NodeComponentLayout(this);
        }

        public override void OnGUI()
        {
            if (node == null)
                return;

            if (parentConnection != null && parentConnection.isDestroyed)
                parentConnection = null;

            layout.Update();

            DoEvents();

            HandleSelection(Selection.gameObjects);

            Color guiColor = GUI.color;

            Color tint = Prefs.dimUnconnectedNodes && node.priority < 1 ? new Color(0.8f, 0.8f, 0.8f, 1f) : Color.white;

            GUI.color = Styles.windowBackground * tint;

            if (
                node.connectionDescriptor == Node.ConnectionDescriptor.OnlyInConnection
                || node.connectionDescriptor == Node.ConnectionDescriptor.Both
            )
                GUI.DrawTextureWithTexCoords(layout.inConnection, Icons.GetResource("in_connection", false),
                    new Rect(0f, 0.5f, 1f, 0.5f));

            GUI.color = Styles.windowBackground * tint;

            if (
                node.connectionDescriptor == Node.ConnectionDescriptor.OnlyOutConnection
                || node.connectionDescriptor == Node.ConnectionDescriptor.Both
            )
                Styles.roundedBox.DrawIfRepaint(layout.outConnectionDraw, false, false, false, false);

            Styles.element.DrawIfRepaint(layout.shadow, false, false, false, false);

            Color c = isSelected ? Prefs.selectionColor : Styles.outlineColor;
            cLerp -= Time.realtimeSinceStartup - t;
            cLerp = Mathf.Clamp01(cLerp);

            GUI.color = Color.Lerp(c, statusColor, cLerp) * tint;
            Styles.outline.DrawIfRepaint(layout.body, false, false, false, false);

            GUI.color = Styles.windowAccent * tint;
            Styles.roundedBox.DrawIfRepaint(layout.content, false, false, false, false);

            GUI.color = Color.white * tint;

            Vector2 mousePos = Event.current.mousePosition;

            GUI.Label(layout.textContent,
                new GUIContent(node.name), Styles.nodeLabel);

            GUI.Label(layout.iconContent,
                new GUIContent(node.icon), Styles.nodeIcon);

            // IEnumerable<Error> errors = new List<Error>() { new Error("Hello World", Error.Severity.Warning) };

            // if (errors.Count() > 0)
            // {
            //     string errorTooltip = String.Join("\n", errors.Select(error => error.message));

            //     GUI.color = Styles.outlineColor;
            //     Styles.roundedBox.DrawIfRepaint(layout.errorBox, false, false, false, false);

            //     Rect tex = layout.errorBox.Pad(4);
            //     tex.position += new Vector2(1, 0);

            //     GUI.color = Color.white;
            //     GUI.Label(tex, new GUIContent(Styles.warnIcon, errorTooltip), GUIStyle.none);
            // }

            if (node.modifiers.Length > 0)
            {
                GUI.color = Styles.outlineColor * tint;
                GUIContent content = new GUIContent("", "Node has active modifiers");
                Styles.roundedBox.DrawIfRepaint(layout.modifierBox, content, false, false, false, false);

                GUI.color = Color.white * tint;
                GUI.DrawTexture(layout.modifierBox.Pad(4), Icons.GetResource("Modifier", false));
            }

            if (node.priority > 0)
            {
                GUI.backgroundColor = Styles.outlineColor * tint;
                Styles.roundedBox.DrawIfRepaint(layout.priorityIndicator, new GUIContent(node.priority.ToString()),
                    false, false, false, false);
                GUI.backgroundColor = Color.white;
            }

            GUI.color = guiColor;

            t = Time.realtimeSinceStartup;
        }

        private void DoEvents()
        {
            Event e = Event.current;
            Vector2 mousePositionGrid = canvas.zoomer.WindowToGridPosition(canvas.mousePositionNoZoom);

            switch (e.rawType)
            {
                case EventType.MouseDown when e.button == 0:
                    beginConnectionOrigin = false;

                    if (layout.inConnection.Contains(e.mousePosition))
                    {
                        if (node.CanHaveParent())
                        {
                            ConnectionComponent.ConnectionComponentCreateArgs createArgs =
                                new ConnectionComponent.ConnectionComponentCreateArgs();
                            createArgs.to = this;

                            parentConnection = floatingConnection = canvas.Create<ConnectionComponent>(createArgs);
                        }
                        else if (parentConnection != null)
                        {
                            parentConnection.to = null;
                            floatingConnection = parentConnection;
                        }

                        e.Use();
                    }
                    else if (layout.outConnection.Contains(e.mousePosition) && node.CanHaveChildren())
                    {
                        ConnectionComponent.ConnectionComponentCreateArgs createArgs =
                            new ConnectionComponent.ConnectionComponentCreateArgs();
                        createArgs.from = this;

                        floatingConnection = canvas.Create<ConnectionComponent>(createArgs);

                        beginConnectionOrigin = true;

                        e.Use();
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

                        if (hoverType == HoverType.InConnection && floatingConnection.to == null &&
                            (hovered.node.CanHaveParent() || hovered.parentConnection == floatingConnection))
                        {
                            floatingConnection.to = hovered;
                        }
                        else if (hoverType == HoverType.OutConnection && floatingConnection.from == null &&
                                 hovered.node.CanHaveChildren())
                        {
                            floatingConnection.from = hovered;
                        }
                        else
                        {
                            floatingConnection.Delete();
                            Destroy(floatingConnection);

                            if (floatingConnection.to == null)
                                DoConnectionDrop(floatingConnection.from, mousePositionGrid);
                        }

                        floatingConnection = null;
                    }
                    else
                    {
                        floatingConnection.Delete();
                        Destroy(floatingConnection);

                        if (floatingConnection.to == null)
                            DoConnectionDrop(floatingConnection.from, mousePositionGrid);

                        floatingConnection = null;
                    }

                    e.Use();
                    break;
                case EventType.ScrollWheel when floatingConnection != null:
                    e.Use();
                    break;
                case EventType.MouseDrag when e.button == 0 && isSelected && canvas.selectionBoxComponent.hidden:
                    float snap = Icons.gridTexture.width / 4f;

                    if (beginDragPosition == null)
                    {
                        beginDragPosition = mousePositionGrid;

                        if (Prefs.gridSnap)
                            node.graphPosition = new Vector2(
                                 Mathf.Round(node.graphPosition.x / snap) * snap,
                                 Mathf.Round(node.graphPosition.y / snap) * snap
                            );

                        beginDragNodePosition = node.graphPosition;
                    }

                    Vector2 dxdy = mousePositionGrid - beginDragPosition.Value;

                    if (Prefs.gridSnap)
                        dxdy = new Vector2(
                             Mathf.Round(dxdy.x / snap) * snap,
                             Mathf.Round(dxdy.y / snap) * snap
                        );

                    node.graphPosition = beginDragNodePosition + dxdy;

                    break;
            }
        }

        private void DoConnectionDrop(NodeComponent old, Vector2 position)
        {
            if (!beginConnectionOrigin)
                return;

            QuickSearch search = new QuickSearch(
                HelperMethods.GetEnumerableOfType(typeof(Node)),
                t =>
                {
                    NodeComponentCreateArgs nodeCreateArgs = new NodeComponentCreateArgs();
                    nodeCreateArgs.graph = node.graph;
                    nodeCreateArgs.nodeType = t;
                    nodeCreateArgs.position = position;

                    NodeComponent n = canvas.Create<NodeComponent>(nodeCreateArgs);
                    n.layout.Update();

                    n.node.graphPosition = new Vector2(n.node.graphPosition.x - n.layout.gridRect.width / 2f,
                        n.node.graphPosition.y);

                    ConnectionComponent.ConnectionComponentCreateArgs connectionCreateArgs =
                        new ConnectionComponent.ConnectionComponentCreateArgs();
                    connectionCreateArgs.from = old;
                    connectionCreateArgs.to = n;
                    connectionCreateArgs.add = true;

                    ConnectionComponent c = canvas.Create<ConnectionComponent>(connectionCreateArgs);
                    n.parentConnection = c;
                }
            );

            WindowComponent.WindowComponentCreateArgs createArgs = new WindowComponent.WindowComponentCreateArgs();

            float xDiff = (canvas.context.GetViewRect().width - 500f) / 2f;
            float yDiff = (canvas.context.GetViewRect().height - 500f) / 2f;

            createArgs.id = 1;
            createArgs.layer = 100;
            createArgs.rect = new Rect(xDiff, yDiff, 500f, 500f);
            createArgs.style = Styles.window;
            createArgs.title = GUIContent.none;
            createArgs.windowProvider = search;
            createArgs.canClose = true;

            canvas.Create<WindowComponent>(createArgs);
        }

        public HoverType GetHoverType(Vector2 mousePosition)
        {
            if (layout.body.Contains(mousePosition))
                return HoverType.Body;
            if (layout.inConnection.Contains(mousePosition))
                return HoverType.InConnection;
            if (layout.outConnection.Contains(mousePosition))
                return HoverType.OutConnection;

            return HoverType.None;
        }

        public override bool ShouldHover(Vector2 mousePosition)
        {
            return layout.body.Contains(mousePosition)
                   || layout.inConnection.Contains(mousePosition)
                   || layout.outConnection.Contains(mousePosition);
        }

        public override bool ResolveObject(Object obj)
        {
            return obj == node;
        }

        public override string GetDebugInfo()
        {
            StringBuilder sb = new StringBuilder();

            if (canvas.hovered == this)
                sb.AppendLine(string.Format("<b>Hovered:</b> {0}", GetHoverType(Event.current.mousePosition)));
            sb.AppendLine(string.Format("<b>Name:</b> {0}", node.name));
            sb.AppendLine(string.Format("<b>GUID:</b> {0}", node.uID));
            sb.AppendLine(string.Format("<b>Status Indicator Enabled:</b> {0}", node.enableStatusIndicator));
            sb.AppendLine(string.Format("<b>Connection Descriptor:</b> {0}", node.connectionDescriptor.ToString()));
            sb.AppendLine(string.Format("<b>Graph Position:</b> {0}", node.graphPosition));
            sb.AppendLine();

            sb.AppendLine("<b>Parent Connection:</b>");
            sb.Append(parentConnection?.GetDebugInfo());
            sb.AppendLine();

            sb.AppendLine(string.Format("<b>Parent:</b> {0}", node.parent?.name));
            sb.Append("<b>Children:</b> ");
            sb.AppendLine(string.Join(", ", node.children.Select(x => x.name)));
            sb.Append("<b>Conditionals:</b> ");
            sb.AppendLine(string.Join(", ", node.conditionals.Select(x => x.name)));
            sb.Append("<b>Modifiers:</b> ");
            sb.AppendLine(string.Join(", ", node.modifiers.Select(x => x.name)));
            sb.AppendLine();

            sb.AppendLine(string.Format("<b>Body:</b> {0}", layout.body));
            sb.AppendLine(string.Format("<b>Content:</b> {0}", layout.textContent));
            sb.AppendLine(string.Format("<b>Shadow:</b> {0}", layout.shadow));
            sb.AppendLine(string.Format("<b>InConnection:</b> {0}", layout.inConnection));
            sb.AppendLine(string.Format("<b>OutConnection:</b> {0}", layout.outConnection));
            sb.AppendLine(string.Format("<b>ErrorBox:</b> {0}", layout.errorBox));
            sb.Append(string.Format("<b>PriorityIndicator:</b> {0}", layout.priorityIndicator));

            return sb.ToString();
        }

        public void HandleSelection(IEnumerable<GameObject> selection)
        {
            if (!Application.isPlaying || !Prefs.enableStatusIndicators || selection.Count() != 1)
            {
                cLerp = 0f;
                return;
            }

            SchemaAgent agent = selection
                .ElementAt(0)
                .GetComponent<SchemaAgent>();

            if (agent == null || agent.target != node.graph)
                return;

            ExecutionContext executionContext = agent.tree.GetExecutionContext(agent);

            switch (executionContext.GetLastStatus(node.priority - 1))
            {
                case NodeStatus.Success:
                    statusColor = Prefs.successColor;
                    cLerp = 1.0f;
                    break;
                case NodeStatus.Failure:
                    statusColor = Prefs.failureColor;
                    cLerp = 1.0f;
                    break;
                case NodeStatus.Running:
                    statusColor = Prefs.highlightColor;
                    cLerp = 1.0f;
                    break;
            }
        }

        public class NodeComponentCreateArgs : CreateArgs
        {
            public Graph graph { get; set; }
            public Node fromExisting { get; set; }
            public Type nodeType { get; set; }
            public Vector2 position { get; set; }
        }

        public class NodeComponentLayout
        {
            public NodeComponentLayout(NodeComponent component)
            {
                this.component = component;
            }

            private NodeComponent component { get; }
            private ShallowNode last { get; set; }
            public Rect gridRect { get; set; }
            public Rect body { get; set; }
            public Rect content { get; set; }
            public Rect textContent { get; set; }
            public Rect iconContent { get; set; }
            public Rect shadow { get; set; }
            public Rect inConnection { get; set; }
            public Rect outConnection { get; set; }
            public Rect outConnectionDraw { get; set; }
            public Rect errorBox { get; set; }
            public Rect priorityIndicator { get; set; }
            public Rect modifierBox { get; set; }
            public Vector2 textSize { get; private set; }
            public Vector2 iconSize { get; private set; }

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
                shadow = body.Pad(-10);
                content = body.Pad(14);
                iconContent = new Rect(content.x + content.width / 2f - iconSize.x / 2f, content.y + ContentPadding.top, iconSize.x, iconSize.y);
                textContent = new Rect(content.x, content.yMax - textSize.y - ContentPadding.bottom, content.width, textSize.y);
                errorBox = new Rect(body.xMax, body.yMax, 28f, 28f).UseCenter();
                inConnection = new Rect(body.center.x, body.y - component.node.conditionals.Length * 50f - 6f, 24f, 12f)
                    .UseCenter();
                outConnection = new Rect(body.center.x, body.yMax + 6f, body.width - 48f, 12f).UseCenter();
                outConnectionDraw = new Rect(outConnection.x, outConnection.y - 12f, outConnection.width, 24f);
                priorityIndicator = new Rect(body.x, body.center.y, v.x, v.y).UseCenter();
                modifierBox = new Rect(body.xMax, body.y, 28f, 28f).UseCenter();
            }

            private void DoRect()
            {
                textSize = Styles.nodeLabel.CalcSize(new GUIContent(component.node.name));

                float width = textSize.x + ContentPadding.left + ContentPadding.right + 28;
                float height = textSize.y + ContentPadding.top + ContentPadding.bottom + 28;

                if (component.node.icon != null)
                {
                    iconSize = Styles.nodeIcon.CalcSize(new GUIContent(component.node.icon));

                    width = Mathf.Max(width, iconSize.x + ContentPadding.left + ContentPadding.right + 28);
                    height += iconSize.y + ContentPadding.bottom;
                }

                gridRect = new Rect(component.node.graphPosition, new Vector2(width, height));
            }

            private struct ShallowNode
            {
                public ShallowNode(Node node)
                {
                    position = node.graphPosition;
                    name = node.name;
                    priority = node.priority;
                }

                public Vector2 position { get; }
                public string name { get; }
                public int priority { get; }

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
        }
    }
}