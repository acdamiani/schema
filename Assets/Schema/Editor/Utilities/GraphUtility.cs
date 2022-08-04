using Schema;
using SchemaEditor;
using SchemaEditor.Internal.ComponentSystem.Components;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public static class GraphUtility
{
    private static Dictionary<string, float> mod = new Dictionary<string, float>();
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
            ConditionalComponent conditionalComponent = NodeEditor.instance.canvas.FindComponent(conditional) as ConditionalComponent;

            if (conditionalComponent == null)
                return Vector2.zero;

            xMin = Mathf.Min(xMin, conditionalComponent.rect.xMin);
            yMin = Mathf.Min(yMin, conditionalComponent.rect.yMin);
            xMax = Mathf.Max(xMax, conditionalComponent.rect.xMax);
            yMax = Mathf.Max(yMax, conditionalComponent.rect.yMax);
        }

        return Rect.MinMaxRect(xMin, yMin, xMax, yMax).size;
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
                node.graphPosition = new Vector2(
                    node.children[0].graphPosition.x + node.children[0].GetSize().x / 2f - nodeSize.x / 2f,
                    0f
                );
            }
            else
            {
                node.graphPosition = Vector2.zero;
            }

            return;
        }

        int nodeIndex = Array.IndexOf(node.parent.children, node);

        if (node.children.Length == 0)
        {
            if (nodeIndex == 0)
                node.graphPosition = new Vector2(0, node.graphPosition.y);
            else
                node.graphPosition = new Vector2(
                    node.parent.children[nodeIndex - 1].graphPosition.x
                        + node.parent.children[nodeIndex - 1].GetSize().x
                        + 25f,
                    node.graphPosition.y
                );
        }
        else if (node.children.Length == 1)
        {
            if (nodeIndex == 0)
            {
                Node child = node.children[0];
                Vector2 childSize = child.GetSize();

                node.graphPosition = new Vector2(child.graphPosition.x + childSize.x / 2f - nodeSize.x / 2f, node.graphPosition.y);
            }
            else
            {
                node.graphPosition = new Vector2(
                    node.parent.children[nodeIndex - 1].graphPosition.x
                        + node.parent.children[nodeIndex - 1].GetSize().x
                        + 25f,
                    node.graphPosition.y
                );
                mod[node.uID] = node.graphPosition.x - node.children[0].graphPosition.x;
            }
        }
        else
        {
            Node first = node.children[0];
            Node last = node.children[node.children.Length - 1];

            float firstCenter = first.graphPosition.x + first.GetSize().x / 2f;
            float lastCenter = last.graphPosition.x + last.GetSize().x / 2f;

            float midpoint = (firstCenter + lastCenter) / 2f - node.GetSize().x / 2f;

            if (nodeIndex == 0)
            {
                node.graphPosition = new Vector2(midpoint, node.graphPosition.y);
            }
            else
            {
                node.graphPosition = new Vector2(
                    node.parent.children[nodeIndex - 1].graphPosition.x
                        + node.parent.children[nodeIndex - 1].GetSize().x
                        + 25f,
                    node.graphPosition.y
                );
                mod[node.uID] = node.graphPosition.x - midpoint;
            }
        }

        if (node.children.Length > 0 && nodeIndex != 0)
            GetOverlapDist(node);

        node.graphPosition = new Vector2(
            node.graphPosition.x,
            node.GetParentSize() + node.GetParentCount() * 100f
        );

        Debug.LogFormat("{0} {1}:{2}", node.name, node.GetSize(), node.GetNodeSize());

        // node.graphPosition += node.GetSize() - node.GetNodeSize();
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

            for (int level = node.GetParentCount() + 1; level <= Math.Min(childContour.Keys.Max(), nodeContour.Keys.Max()); level++)
            {
                float distance = nodeContour[level] - childContour[level];

                shift = -distance + 25f;
            }

            if (shift > 0f)
            {
                node.graphPosition = new Vector2(node.graphPosition.x + shift, node.graphPosition.y);
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
            values.Add(pCount, node.graphPosition.x + sum);
        else
            values[pCount] = Math.Max(values[pCount], node.graphPosition.x + sum + node.GetSize().x);

        mod.TryGetValue(node.uID, out float v);
        sum += v;

        foreach (Node child in node.children)
            GetRightContour(child, sum, ref values);
    }
    private static void GetLeftContour(Node node, float sum, ref Dictionary<int, float> values)
    {
        int pCount = node.GetParentCount();

        if (!values.ContainsKey(pCount))
            values.Add(pCount, node.graphPosition.x + sum);
        else
            values[pCount] = Math.Min(values[pCount], node.graphPosition.x + sum);

        mod.TryGetValue(node.uID, out float v);
        sum += v;

        foreach (Node child in node.children)
            GetLeftContour(child, sum, ref values);
    }
    private static void Center(Node left, Node right)
    {
        int leftIndex = Array.IndexOf(left.parent.children, left);
        int rightIndex = Array.IndexOf(left.parent.children, right);

        int numBetween = (rightIndex - leftIndex) - 1;

        if (numBetween > 0)
        {
            float leftWidth = left.GetSize().x;

            float dist = ((right.graphPosition.x) - (left.graphPosition.x + leftWidth));
            float areaBetween = 0f;

            for (int i = leftIndex + 1; i < rightIndex; i++)
                areaBetween += left.parent.children[i].GetSize().x;

            float step = (dist - areaBetween) / (numBetween + 1);
            float x = step;

            for (int i = leftIndex + 1; i < rightIndex; i++)
            {
                Node middle = left.parent.children[i];

                float final = left.graphPosition.x + leftWidth + x;
                float diff = final - middle.graphPosition.x;

                middle.graphPosition = new Vector2(middle.graphPosition.x + diff, middle.graphPosition.y);
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
        root.graphPosition = new Vector2(root.graphPosition.x + value, root.graphPosition.y);

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