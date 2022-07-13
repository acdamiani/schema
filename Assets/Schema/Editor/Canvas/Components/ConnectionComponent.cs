using Schema;
using SchemaEditor;
using SchemaEditor.Internal;
using UnityEngine;
using UnityEditor;
using System;

public sealed class ConnectionComponent : GUIComponent
{
    public NodeComponent from { get; set; }
    public NodeComponent to { get; set; }
    public override void Create(CreateArgs args)
    {
        ConnectionComponentCreateArgs createArgs = args as ConnectionComponentCreateArgs;

        if (createArgs == null)
            throw new ArgumentException();

        from = createArgs.from;
        to = createArgs.to;

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
    public void Join()
    {
        if (from == null || to == null)
            return;

        from.node.AddConnection(to.node);
    }
    public class ConnectionComponentCreateArgs : CreateArgs
    {
        public NodeComponent from { get; set; }
        public NodeComponent to { get; set; }
    }
}