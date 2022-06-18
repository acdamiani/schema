using Schema;
using Schema.Utilities;
using SchemaEditor.Utilities;
using SchemaEditor.Internal;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public sealed class NodeComponent : GUIComponent
{
    private static readonly RectOffset ContentPadding = new RectOffset(20, 20, 14, 14);
    private Node node;
    private bool logOnce;
    public override void Create(CreateArgs args)
    {
        NodeComponentCreateArgs createArgs = args as NodeComponentCreateArgs;

        if (createArgs == null)
            throw new ArgumentException();

        if (createArgs.fromExisting != null)
            node = createArgs.fromExisting;
        else
            node = createArgs.graph.AddNode(createArgs.nodeType, createArgs.position);
    }
    public override void OnGUI()
    {
        Rect r = rect.UseCenter();

        Color guiColor = GUI.color;

        GUI.color = Styles.windowBackground;
        Styles.shadow.DrawIfRepaint(r.Pad(new RectOffset(-14, -14, -14, -14)), false, false, false, false);

        GUI.color = Styles.windowBackground;
        GUI.DrawTexture(new Rect(r.center.x, r.y, 24f, 24f).UseCenter(), Styles.circle);

        GUI.color = Styles.outlineColor;
        Styles.styles.nodeSelected.DrawIfRepaint(r, false, false, false, false);

        Rect content = r.Pad(new RectOffset(14, 14, 14, 14));

        GUI.color = Styles.windowAccent;
        Styles.roundedBox.DrawIfRepaint(content, false, false, false, false);

        GUI.color = Color.white;

        GUILayout.BeginArea(content);
        GUILayout.Space(ContentPadding.top);

        GUILayout.Label(new GUIContent(node.name, node.icon), Styles.nodeLabel);

        GUILayout.Space(ContentPadding.bottom);
        GUILayout.EndArea();

        Rect warningRect = new Rect(r.xMax, r.yMax, 40f, 40f).UseCenter();

        List<Error> errors = node.GetErrors();

        if (errors.Count > 0)
        {
            string errorTooltip = String.Join("\n", errors.Select(error => error.message));

            GUI.color = Styles.outlineColor;
            Styles.roundedBox.DrawIfRepaint(warningRect, false, false, false, false);

            Rect tex = warningRect.Pad(4);
            tex.position += new Vector2(1, 0);

            GUI.color = Color.white;
            GUI.Label(tex, new GUIContent(Styles.warnIcon, errorTooltip), GUIStyle.none);
        }

        GUI.color = guiColor;
    }
    protected override Rect GetRect()
    {
        Vector2 labelSize = Styles.nodeLabel.CalcSize(new GUIContent(node.name, node.icon));

        float width = labelSize.x + ContentPadding.left + ContentPadding.right + 28;
        float height = labelSize.y + ContentPadding.top + ContentPadding.bottom + 28;

        return new Rect(node.graphPosition - new Vector2(250f, 0f), new Vector2(width, height));
    }
    public class NodeComponentCreateArgs : CreateArgs
    {
        public Graph graph { get; set; }
        public Node fromExisting { get; set; }
        public Type nodeType { get; set; }
        public Vector2 position { get; set; }
    }
}