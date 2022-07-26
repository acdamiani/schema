using Schema;
using SchemaEditor;
using SchemaEditor.Internal;
using UnityEngine;
using UnityEditor;
using SchemaEditor.Internal.ComponentSystem;
using System;

public sealed class ConnectionComponent : GUIComponent, IViewElement
{
    public NodeComponent from
    {
        get { return _from; }
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
        get { return _to; }
        set
        {
            _to = value;

            if (_to == null && _from == null)
                Destroy(this);
            else if (_to != null)
                connectionTo = _to;
        }
    }
    private NodeComponent _from;
    private NodeComponent _to;
    private NodeComponent connectionFrom
    {
        get { return _connectionFrom; }
        set
        {
            if (value != _connectionFrom && _connectionTo != null)
            {
                value.node.AddConnection(_connectionTo.node);
                value.node.graph.Traverse();
            }

            _connectionFrom = value;
        }
    }
    private NodeComponent _connectionFrom;
    private NodeComponent connectionTo
    {
        get { return _connectionTo; }
        set
        {
            if (value != _connectionTo && _connectionFrom != null)
            {
                value.parentConnection = this;
                if (_connectionTo != null)
                    _connectionTo.parentConnection = null;
                _connectionFrom.node.AddConnection(value.node);
                _connectionFrom.node.graph.Traverse();
            }

            _connectionTo = value;
        }
    }
    private NodeComponent _connectionTo;
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
    }
    public override void OnGUI()
    {
        Vector2 p0;
        Vector2 p3;

        if (from == null)
            p0 = Event.current.mousePosition;
        else
            p0 = new Vector2(from.layout.body.center.x, from.layout.body.yMax + 12f);

        if (to == null)
            p3 = Event.current.mousePosition;
        else
            p3 = new Vector2(to.layout.body.center.x, to.layout.body.y - 12f);

        float vertDist = Mathf.Abs(p0.y - p3.y);

        Vector2 p1 = p0 + Vector2.up * vertDist / 2f;
        Vector2 p2 = p3 - Vector2.up * vertDist / 2f;

        Handles.DrawBezier(p0, p3, p1, p2, Color.white, Styles.curve, NodeEditor.instance.windowInfo.zoom * 5f);
    }
    protected override void OnDestroy()
    {
        if (connectionFrom == null || connectionTo == null)
            return;

        connectionFrom.node.RemoveConnection(connectionTo.node);
    }
    public class ConnectionComponentCreateArgs : CreateArgs
    {
        public NodeComponent from { get; set; }
        public NodeComponent to { get; set; }
    }
}