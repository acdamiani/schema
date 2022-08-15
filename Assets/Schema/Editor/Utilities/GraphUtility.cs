using System;
using System.Collections.Generic;
using System.Linq;
using Schema;
using SchemaEditor;
using SchemaEditor.Internal;
using SchemaEditor.Internal.ComponentSystem.Components;
using UnityEngine;

public static class GraphUtility
{
    private static readonly Dictionary<string, float> mod = new();

    public static void Arrange(IEnumerable<Node> nodes)
    {
        Node.BeginPosNoCheck();

        Node root = nodes.Where(node => node.GetType() == typeof(Root)).FirstOrDefault();

        Calc(root);
        MoveTree(root, 0f);

        Node.EndPosNoCheck();
    }

    private static Vector2 GetNodeSize(this Node node)
    {
        NodeComponent nodeComponent = NodeEditor.instance.canvas.FindComponent(node) as NodeComponent;

        if (nodeComponent == null)
            return Vector2.zero;

        return nodeComponent.layout.body.size;
    }

    private static Vector2 GetSize(this Node node)
    {
        NodeComponent nodeComponent = NodeEditor.instance.canvas.FindComponent(node) as NodeComponent;

        if (nodeComponent == null)
            return Vector2.zero;

        float xMin = nodeComponent.layout.body.xMin;
        float yMin = nodeComponent.layout.body.yMin;
        float xMax = nodeComponent.layout.body.xMax;
        float yMax = nodeComponent.layout.body.yMax;

        foreach (Conditional conditional in node.conditionals)
        {
            ConditionalComponent conditionalComponent =
                NodeEditor.instance.canvas.FindComponent(conditional) as ConditionalComponent;

            if (conditionalComponent == null)
                return Vector2.zero;

            xMin = Mathf.Min(xMin, conditionalComponent.GetRect().xMin);
            yMin = Mathf.Min(yMin, conditionalComponent.GetRect().yMin);
            xMax = Mathf.Max(xMax, conditionalComponent.GetRect().xMax);
            yMax = Mathf.Max(yMax, conditionalComponent.GetRect().yMax);
        }

        return (new Vector2(xMax, yMax) - new Vector2(xMin, yMin)) * NodeEditor.instance.canvas.zoomer.zoom;
    }

    private static void SetPosition(this Node node, Vector2 position)
    {
        NodeComponent nodeComponent = NodeEditor.instance.canvas.FindComponent(node) as NodeComponent;

        if (nodeComponent == null)
            return;

        Vector2 min = node.GetPosition();

        node.graphPosition = position - min + node.graphPosition;
        nodeComponent.layout.Update();
    }

    private static Vector2 GetPosition(this Node node)
    {
        NodeComponent nodeComponent = NodeEditor.instance.canvas.FindComponent(node) as NodeComponent;

        if (nodeComponent == null)
            return Vector2.zero;

        float xMin = nodeComponent.layout.body.xMin;
        float yMin = nodeComponent.layout.body.yMin;

        foreach (Conditional conditional in node.conditionals)
        {
            ConditionalComponent conditionalComponent =
                NodeEditor.instance.canvas.FindComponent(conditional) as ConditionalComponent;

            if (conditionalComponent == null)
                return Vector2.zero;

            xMin = Mathf.Min(xMin, conditionalComponent.GetRect().xMin);
            yMin = Mathf.Min(yMin, conditionalComponent.GetRect().yMin);
        }

        Vector2 min = NodeEditor.instance.canvas.zoomer.WindowToGridPosition(new Vector2(xMin, yMin));

        return min;
    }

    private static void Calc(Node node)
    {
        foreach (Node child in node.children)
            Calc(child);

        Vector2 nodeSize = node.GetSize();

        if (node.GetType() == typeof(Root))
        {
            if (node.children.Length > 0)
            {
                Vector2 v = new Vector2(
                    node.children[0].GetPosition().x + node.children[0].GetSize().x / 2f - nodeSize.x / 2f,
                    0f
                );
                node.SetPosition(v);
            }
            else
            {
                node.SetPosition(Vector2.zero);
            }

            return;
        }

        int nodeIndex = Array.IndexOf(node.parent.children, node);

        if (node.children.Length == 0)
        {
            if (nodeIndex == 0)
                node.SetPosition(new Vector2(0, node.GetPosition().y));
            else
                node.SetPosition(new Vector2(
                    node.parent.children[nodeIndex - 1].GetPosition().x
                    + node.parent.children[nodeIndex - 1].GetSize().x
                    + Prefs.arrangeHorizontalSpacing,
                    node.GetPosition().y
                ));
        }
        else if (node.children.Length == 1)
        {
            if (nodeIndex == 0)
            {
                Node child = node.children[0];
                Vector2 childSize = child.GetSize();

                node.SetPosition(new Vector2(child.GetPosition().x + childSize.x / 2f - nodeSize.x / 2f,
                    node.GetPosition().y));
            }
            else
            {
                node.SetPosition(new Vector2(
                    node.parent.children[nodeIndex - 1].GetPosition().x
                    + node.parent.children[nodeIndex - 1].GetSize().x
                    + Prefs.arrangeHorizontalSpacing,
                    node.GetPosition().y
                ));
                mod[node.uID] = node.GetPosition().x - node.children[0].GetPosition().x;
            }
        }
        else
        {
            Node first = node.children[0];
            Node last = node.children[node.children.Length - 1];

            float firstCenter = first.GetPosition().x + first.GetSize().x / 2f;
            float lastCenter = last.GetPosition().x + last.GetSize().x / 2f;

            float midpoint = (firstCenter + lastCenter) / 2f - node.GetSize().x / 2f;

            if (nodeIndex == 0)
            {
                node.SetPosition(new Vector2(midpoint, node.GetPosition().y));
            }
            else
            {
                node.SetPosition(new Vector2(
                    node.parent.children[nodeIndex - 1].GetPosition().x
                    + node.parent.children[nodeIndex - 1].GetSize().x
                    + Prefs.arrangeHorizontalSpacing,
                    node.GetPosition().y
                ));
                mod[node.uID] = node.GetPosition().x - midpoint;
            }
        }

        if (node.children.Length > 0 && nodeIndex != 0)
            GetOverlapDist(node);

        node.SetPosition(new Vector2(
            node.GetPosition().x,
            node.GetParentSize() + node.GetParentCount() * Prefs.arrangeVerticalSpacing
        ));
    }

    private static void GetOverlapDist(Node node)
    {
        if (node.parent == null)
            return;

        int nodeIndex = Array.IndexOf(node.parent.children, node);

        Dictionary<int, float> nodeContour = new Dictionary<int, float>();

        float shift = 0f;

        GetLeftContour(node, 0f, ref nodeContour);

        for (int i = 0; i < nodeIndex; i++)
        {
            Dictionary<int, float> childContour = new Dictionary<int, float>();
            GetRightContour(node.parent.children[i], 0f, ref childContour);

            for (int level = node.GetParentCount() + 1;
                 level <= Math.Min(childContour.Keys.Max(), nodeContour.Keys.Max());
                 level++)
            {
                float distance = nodeContour[level] - childContour[level];

                shift = -distance + Prefs.arrangeHorizontalSpacing;
            }

            if (shift > 0f)
            {
                node.SetPosition(new Vector2(node.GetPosition().x + shift, node.GetPosition().y));
                mod.TryGetValue(node.uID, out float v);
                mod[node.uID] = v + shift;

                Center(node.parent.children[i], node);

                shift = 0;
            }
        }
    }

    private static IEnumerable<Node> PreOrder(Node root)
    {
        List<Node> ret = new List<Node>();

        if (root.children.Length == 0)
        {
            ret.Add(root);
            return ret;
        }

        foreach (Node child in root.children)
            ret.AddRange(PreOrder(child));

        ret.Add(root);

        return ret;
    }

    private static void GetRightContour(Node node, float sum, ref Dictionary<int, float> values)
    {
        int pCount = node.GetParentCount();

        if (!values.ContainsKey(pCount))
            values.Add(pCount, node.GetPosition().x + sum);
        else
            values[pCount] = Math.Max(values[pCount], node.GetPosition().x + sum + node.GetSize().x);

        mod.TryGetValue(node.uID, out float v);
        sum += v;

        foreach (Node child in node.children)
            GetRightContour(child, sum, ref values);
    }

    private static void GetLeftContour(Node node, float sum, ref Dictionary<int, float> values)
    {
        int pCount = node.GetParentCount();

        if (!values.ContainsKey(pCount))
            values.Add(pCount, node.GetPosition().x + sum);
        else
            values[pCount] = Math.Min(values[pCount], node.GetPosition().x + sum);

        mod.TryGetValue(node.uID, out float v);
        sum += v;

        foreach (Node child in node.children)
            GetLeftContour(child, sum, ref values);
    }

    private static void Center(Node left, Node right)
    {
        int leftIndex = Array.IndexOf(left.parent.children, left);
        int rightIndex = Array.IndexOf(left.parent.children, right);

        int numBetween = rightIndex - leftIndex - 1;

        if (numBetween > 0)
        {
            float leftWidth = left.GetSize().x;

            float dist = right.GetPosition().x - (left.GetPosition().x + leftWidth);
            float areaBetween = 0f;

            for (int i = leftIndex + 1; i < rightIndex; i++)
                areaBetween += left.parent.children[i].GetSize().x;

            float step = (dist - areaBetween) / (numBetween + 1);
            float x = step;

            for (int i = leftIndex + 1; i < rightIndex; i++)
            {
                Node middle = left.parent.children[i];

                float final = left.GetPosition().x + leftWidth + x;
                float diff = final - middle.GetPosition().x;

                middle.SetPosition(new Vector2(middle.GetPosition().x + diff, middle.GetPosition().y));
                mod.TryGetValue(middle.uID, out float v);
                mod[middle.uID] = v + diff;

                x += step + middle.GetSize().x;
            }

            GetOverlapDist(left);
        }
    }

    private static int GetParentCount(this Node node)
    {
        int count = 0;
        Node current = node;

        while (current.parent != null)
        {
            current = current.parent;
            count++;
        }

        return count;
    }

    private static float GetParentSize(this Node node)
    {
        float size = 0f;
        Node current = node;

        while (current.parent != null)
        {
            current = current.parent;
            size += current.GetSize().y;
        }

        return size;
    }

    private static void MoveTree(Node root, float value)
    {
        root.SetPosition(new Vector2(root.GetPosition().x + value, root.GetPosition().y));

        mod.TryGetValue(root.uID, out float v);
        value += v;

        foreach (Node child in root.children)
            MoveTree(child, value);
    }

    private static float GetTotalMod(Node target, float value)
    {
        mod.TryGetValue(target.uID, out float v);
        value += v;

        if (target.parent == null)
            return value;

        return GetTotalMod(target.parent, value);
    }
}