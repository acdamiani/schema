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
        Node root = nodes.Where(node => node.GetType() == typeof(Root)).FirstOrDefault();

        Calc(root);
        MoveTree(root, 0f);
    }
    private static void Calc(Node node)
    {
        foreach (Node child in node.children) Calc(child);

        Vector2 nodeSize = NodeEditor.GetAreaWithPadding(node, false);

        if (node.GetType() == typeof(Root))
        {
            node.position = new Vector2(
                node.children[0].position.x + NodeEditor.GetAreaWithPadding(node.children[0], false).x / 2f - nodeSize.x / 2f,
                node.position.y
            );

            return;
        }

        int nodeIndex = Array.IndexOf(node.parent.children, node);

        if (node.children.Length == 0)
        {
            if (nodeIndex == 0)
                node.position = new Vector2(0, node.position.y);
            else
                node.position = new Vector2(
                    node.parent.children[nodeIndex - 1].position.x
                        + NodeEditor.GetAreaWithPadding(node.parent.children[nodeIndex - 1], false).x
                        + 25f,
                    node.position.y
                );
        }
        else if (node.children.Length == 1)
        {
            if (nodeIndex == 0)
            {
                Node child = node.children[0];
                Vector2 childSize = NodeEditor.GetAreaWithPadding(child, false);

                node.position = new Vector2(child.position.x + childSize.x / 2f - nodeSize.x / 2f, node.position.y);
            }
            else
            {
                node.position = new Vector2(
                    node.parent.children[nodeIndex - 1].position.x
                        + NodeEditor.GetAreaWithPadding(node.parent.children[nodeIndex - 1], false).x
                        + 25f,
                    node.position.y
                );
                mod[node.uID] = node.position.x - node.children[0].position.x;
            }
        }
        else
        {
            Node first = node.children[0];
            Node last = node.children[node.children.Length - 1];

            float firstCenter = first.position.x + NodeEditor.GetAreaWithPadding(first, false).x / 2f;
            float lastCenter = last.position.x + NodeEditor.GetAreaWithPadding(last, false).x / 2f;

            float midpoint = (firstCenter + lastCenter) / 2f - NodeEditor.GetAreaWithPadding(node, false).x / 2f;

            if (nodeIndex == 0)
            {
                node.position = new Vector2(midpoint, node.position.y);
            }
            else
            {
                node.position = new Vector2(
                    node.parent.children[nodeIndex - 1].position.x
                        + NodeEditor.GetAreaWithPadding(node.parent.children[nodeIndex - 1], false).x
                        + 25f,
                    node.position.y
                );
                mod[node.uID] = node.position.x - midpoint;
            }
        }

        if (node.children.Length > 0 && nodeIndex != 0)
            GetOverlapDist(node);

        node.position = new Vector2(node.position.x, node.GetParentCount() * 300f);
    }
    private static void GetOverlapDist(Node node)
    {
        if (node.parent == null)
            return;

        float minDistance = NodeEditor.GetAreaWithPadding(node, false).x + 25f;
        int nodeIndex = Array.IndexOf(node.parent.children, node);

        Dictionary<int, float> nodeContour = new Dictionary<int, float>();

        float shift = 0f;

        GetLeftContour(node, 0f, ref nodeContour);

        if (node.GetType() == typeof(SelectRandomWeighted))
        {
            Debug.Log(String.Join(", ", nodeContour.Select(v => v.Key + ": " + v.Value)));
        }

        for (int i = 0; i < nodeIndex; i++)
        {
            Dictionary<int, float> childContour = new Dictionary<int, float>();
            GetRightContour(node.parent.children[i], 0f, ref childContour);

            for (int level = node.GetParentCount() + 1; level <= Math.Min(childContour.Keys.Max(), nodeContour.Keys.Max()); level++)
            {
                float distance = nodeContour[level] - childContour[level];

                if (distance + shift < minDistance)
                    shift = minDistance - distance;
            }

            if (shift > 0f)
            {
                node.position = new Vector2(node.position.x + shift, node.position.y);
                mod.TryGetValue(node.uID, out float v);
                mod[node.uID] = v + shift;

                Center(node.parent.children[i], node);

                shift = 0;
            }
        }
    }
    private static void GetRightContour(Node node, float sum, ref Dictionary<int, float> values)
    {
        int pCount = node.GetParentCount();

        if (!values.ContainsKey(pCount))
            values.Add(pCount, node.position.x + sum);
        else
            values[pCount] = Math.Max(values[pCount], node.position.x + sum + NodeEditor.GetAreaWithPadding(node, false).x);

        mod.TryGetValue(node.uID, out float v);
        sum += v;

        foreach (Node child in node.children)
            GetRightContour(child, sum, ref values);
    }
    private static void GetLeftContour(Node node, float sum, ref Dictionary<int, float> values)
    {
        /*           
            if (!values.ContainsKey(node.Y))
                values.Add(node.Y, node.X + modSum);
            else
                values[node.Y] = Math.Min(values[node.Y], node.X + modSum);
 
            modSum += node.Mod;
            foreach (var child in node.Children)
            {
                GetLeftContour(child, modSum, ref values);
            }
        */

        int pCount = node.GetParentCount();

        if (!values.ContainsKey(pCount))
            values.Add(pCount, node.position.x + sum);
        else
            values[pCount] = Math.Min(values[pCount], node.position.x + sum);

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

            float dist = ((right.position.x) - (left.position.x + leftWidth));
            float areaBetween = 0f;

            for (int i = leftIndex + 1; i < rightIndex; i++)
                areaBetween += NodeEditor.GetAreaWithPadding(left.parent.children[i], false).x;

            float step = (dist - areaBetween) / (numBetween + 1);
            float x = step;

            for (int i = leftIndex + 1; i < rightIndex; i++)
            {
                Node middle = left.parent.children[i];

                float final = left.position.x + leftWidth + x;
                float diff = final - middle.position.x;

                middle.position = new Vector2(middle.position.x + diff, middle.position.y);
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

        root.position = new Vector2(root.position.x + value, root.position.y);

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