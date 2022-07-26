using Schema;
using SchemaEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public static class GraphUtility
{

    private static Dictionary<string, float> mod = new Dictionary<string, float>();
    public static void Prettify(IEnumerable<Node> nodes)
    {
        nodes = PreOrder(nodes.Aggregate((n1, n2) => n1.priority < n2.priority ? n1 : n2));

        Node root = nodes.Where(node => node.GetType() == typeof(Root)).FirstOrDefault();

        Calc(root);
        MoveTree(root, 0f);
    }
    private static void Calc(Node node)
    {
        foreach (Node child in node.children)
            Calc(child);

        Vector2 nodeSize = NodeEditor.GetAreaWithPadding(node, false);

        if (node.GetType() == typeof(Root))
        {
            node.graphPosition = new Vector2(
                node.children[0].graphPosition.x + NodeEditor.GetAreaWithPadding(node.children[0], false).x / 2f - nodeSize.x / 2f,
                node.graphPosition.y
            );

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
                        + NodeEditor.GetAreaWithPadding(node.parent.children[nodeIndex - 1], false).x
                        + 25f,
                    node.graphPosition.y
                );
        }
        else if (node.children.Length == 1)
        {
            if (nodeIndex == 0)
            {
                Node child = node.children[0];
                Vector2 childSize = NodeEditor.GetAreaWithPadding(child, false);

                node.graphPosition = new Vector2(child.graphPosition.x + childSize.x / 2f - nodeSize.x / 2f, node.graphPosition.y);
            }
            else
            {
                node.graphPosition = new Vector2(
                    node.parent.children[nodeIndex - 1].graphPosition.x
                        + NodeEditor.GetAreaWithPadding(node.parent.children[nodeIndex - 1], false).x
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

            float firstCenter = first.graphPosition.x + NodeEditor.GetAreaWithPadding(first, false).x / 2f;
            float lastCenter = last.graphPosition.x + NodeEditor.GetAreaWithPadding(last, false).x / 2f;

            float midpoint = (firstCenter + lastCenter) / 2f - NodeEditor.GetAreaWithPadding(node, false).x / 2f;

            if (nodeIndex == 0)
            {
                node.graphPosition = new Vector2(midpoint, node.graphPosition.y);
            }
            else
            {
                node.graphPosition = new Vector2(
                    node.parent.children[nodeIndex - 1].graphPosition.x
                        + NodeEditor.GetAreaWithPadding(node.parent.children[nodeIndex - 1], false).x
                        + 25f,
                    node.graphPosition.y
                );
                mod[node.uID] = node.graphPosition.x - midpoint;
            }
        }

        if (node.children.Length > 0 && nodeIndex != 0)
        {
            GetOverlapDist(node);
        }

        node.graphPosition = new Vector2(node.graphPosition.x, node.GetParentCount() * 300f);
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
            values[pCount] = Math.Max(values[pCount], node.graphPosition.x + sum + NodeEditor.GetAreaWithPadding(node, false).x);

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
            float leftWidth = NodeEditor.GetAreaWithPadding(left, false).x;

            float dist = ((right.graphPosition.x) - (left.graphPosition.x + leftWidth));
            float areaBetween = 0f;

            for (int i = leftIndex + 1; i < rightIndex; i++)
                areaBetween += NodeEditor.GetAreaWithPadding(left.parent.children[i], false).x;

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

                x += step + NodeEditor.GetAreaWithPadding(middle, false).x;
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
    private static void MoveTree(Node root, float value)
    {
        if (root.GetType() == typeof(CustomAction))
            Debug.Log(value);

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