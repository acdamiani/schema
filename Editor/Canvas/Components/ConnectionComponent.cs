using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Schema.Utilities;
using UnityEditor;
using UnityEngine;

namespace SchemaEditor.Internal.ComponentSystem.Components
{
    public sealed class ConnectionComponent : GUIComponent, IViewElement, ISelectable, IDeletable
    {
        private NodeComponent _connectionFrom;
        private NodeComponent _connectionTo;
        private NodeComponent _from;
        private NodeComponent _to;
        private Bezier curve;
        public bool isSelected { get; private set; }

        public NodeComponent from
        {
            get => _from;
            set
            {
                _from = value;

                if (_from == null && _to == null)
                    Destroy(this);
                else if (_from != null)
                    connectionFrom = _from;
            }
        }

        public NodeComponent to
        {
            get => _to;
            set
            {
                _to = value;

                if (_to == null && _from == null)
                    Destroy(this);
                else if (_to != null)
                    connectionTo = _to;
            }
        }

        private NodeComponent connectionFrom
        {
            get => _connectionFrom;
            set
            {
                if (value != _connectionFrom && _connectionTo != null)
                {
                    if (_connectionFrom != null)
                        _connectionFrom.node.RemoveConnection(_connectionTo.node);
                    value.node.AddConnection(_connectionTo.node);
                    value.node.graph.Traverse();
                }

                _connectionFrom = value;
            }
        }

        private NodeComponent connectionTo
        {
            get => _connectionTo;
            set
            {
                if (value != _connectionTo && _connectionFrom != null)
                {
                    if (_connectionTo != null)
                        _connectionFrom.node.RemoveConnection(_connectionTo.node);
                    if (value.parentConnection != null)
                        Destroy(value.parentConnection);
                    value.parentConnection = this;
                    if (_connectionTo != null)
                        _connectionTo.parentConnection = null;
                    _connectionFrom.node.AddConnection(value.node);
                    _connectionFrom.node.graph.Traverse();
                }

                _connectionTo = value;
            }
        }

        public bool IsDeletable()
        {
            IEnumerable<NodeComponent> nodeComponents = canvas.selected
                .Where(x => x is NodeComponent)
                .Cast<NodeComponent>();

            return isSelected && !nodeComponents.Contains(connectionTo) && !nodeComponents.Contains(connectionFrom);
        }

        public void Delete()
        {
            if (connectionFrom == null || connectionTo == null)
                return;

            connectionFrom.node?.RemoveConnection(connectionTo.node);
        }

        public bool IsSelectable()
        {
            return true;
        }

        public bool IsSelected()
        {
            return isSelected;
        }

        public void Select(bool additive)
        {
            isSelected = true;
        }

        public void Deselect()
        {
            isSelected = false;
        }

        public bool Overlaps(Rect rect)
        {
            return curve.Intersect(rect);
        }

        public bool IsHit(Vector2 position)
        {
            return curve.Hit(position, 5f * canvas.zoomer.zoom);
        }

        public override void Create(CreateArgs args)
        {
            ConnectionComponentCreateArgs createArgs = args as ConnectionComponentCreateArgs;

            if (createArgs == null)
                throw new ArgumentException();

            _from = createArgs.from;
            _to = createArgs.to;

            _connectionFrom = createArgs.from;
            _connectionTo = createArgs.to;

            if (from == null && to == null)
                throw new ArgumentNullException();

            if (createArgs.add)
                _from.node.AddConnection(_to.node);

            curve = new Bezier(Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
        }

        public override void OnGUI()
        {
            if (
                (connectionFrom != null && connectionFrom.node == null)
                || (connectionTo != null && connectionTo.node == null)
            )
            {
                Destroy(this);
                return;
            }

            Vector2 p0;
            Vector2 p3;

            NodeComponent hovered = canvas.hovered as NodeComponent;

            if (from == null)
                p0 = hovered != null && hovered.GetHoverType(Event.current.mousePosition) ==
                    NodeComponent.HoverType.OutConnection
                        ? new Vector2(hovered.layout.outConnection.center.x, hovered.layout.outConnection.yMax)
                        : Event.current.mousePosition;
            else
                p0 = new Vector2(from.layout.outConnection.center.x, from.layout.outConnection.yMax);

            if (to == null)
                p3 = hovered != null && hovered.GetHoverType(Event.current.mousePosition) ==
                    NodeComponent.HoverType.InConnection
                        ? new Vector2(hovered.layout.inConnection.center.x, hovered.layout.inConnection.y)
                        : Event.current.mousePosition;
            else
                p3 = new Vector2(to.layout.inConnection.center.x, to.layout.inConnection.y);

            float vertDist = Mathf.Abs(p0.y - p3.y);

            Vector2 p1 = p0 + Vector2.up * vertDist / 2f;
            Vector2 p2 = p3 - Vector2.up * vertDist / 2f;

            curve.p0 = p0;
            curve.p1 = p1;
            curve.p2 = p2;
            curve.p3 = p3;

            Handles.DrawBezier(p0, p3, p1, p2, isSelected ? Prefs.selectionColor : Color.gray,
                Icons.GetResource("curve", false),
                canvas.zoomer.zoom * 3f);
        }

        public override string GetDebugInfo()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format("<b>From:</b> {0}", from?.node.name));
            sb.AppendLine(string.Format("<b>To:</b> {0}", to?.node.name));

            return sb.ToString();
        }

        public class ConnectionComponentCreateArgs : CreateArgs
        {
            public NodeComponent from { get; set; }
            public NodeComponent to { get; set; }
            public bool add { get; set; }
        }
    }
}