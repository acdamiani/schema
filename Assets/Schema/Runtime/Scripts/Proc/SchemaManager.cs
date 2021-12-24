using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Schema.Runtime;
public static class SchemaManager
{
    private static Dictionary<Graph, OptimizedGraph> map = new Dictionary<Graph, OptimizedGraph>();
    public static OptimizedGraph LoadGraph(Graph graph, bool reload)
    {
        if (!map.ContainsKey(graph) || reload)
        {
            OptimizedGraph optimizedGraph = new OptimizedGraph();
            Graph loadedGraph = LoadIntoMemory(graph);

            optimizedGraph.nodes = loadedGraph.nodes.Flatten();
            map[graph] = optimizedGraph;

            return optimizedGraph;
        }
        else
        {
            return map[graph];
        }
    }
    public static Graph LoadIntoMemory(Graph graph)
    {
        Graph ret = ScriptableObject.Instantiate(graph);
        List<Node> topLevel = graph.nodes.Where(node => node.parent == null).ToList();

        ret.nodes.Clear();

        for (int i = 0; i < topLevel.Count; i++)
        {
            DuplicateRecursive(ret.nodes, null, topLevel[i]);
        }

        ret.root = (Root)ret.nodes.Find(node => node.GetType() == typeof(Root));

        try
        {
            ret.blackboard = ScriptableObject.Instantiate(graph.blackboard);
            ret.blackboard.name = "Blackboard";
            ret.blackboard.entries = graph.blackboard.entries.Select(entry =>
            {
                BlackboardEntry cache = ScriptableObject.Instantiate(entry);
                cache.name = cache.Name;
                return cache;
            }).ToList();
        }
        catch { }

        return ret;
    }
    private static Node DuplicateRecursive(List<Node> graph, Node parent, Node dupl)
    {
        Node ret = ScriptableObject.Instantiate(dupl);

        for (int i = 0; i < ret.decorators.Count; i++)
        {
            ret.decorators[i] = ScriptableObject.Instantiate(ret.decorators[i]);
            ret.decorators[i].node = ret;
        }

        ret.parent = parent;
        ret.children = ret.children.Select(node => DuplicateRecursive(graph, ret, node)).ToList();

        graph.Add(ret);

        return ret;
    }

    private static OptimizedNode[] Flatten(this List<Node> nodes)
    {
        return Flatten(nodes.ToArray());
    }
    private static OptimizedNode[] Flatten(this Node[] nodes)
    {
        List<Node> working = new List<Node>(nodes);

        //First, create an array Ordered by priority
        working.RemoveAll(node => node.priority < 1);
        nodes = working.OrderBy(node => node.priority).ToArray();

        OptimizedNode[] cache = new OptimizedNode[nodes.Length];

        //Update breadth
        for (int i = 0; i < nodes.Length; i++)
        {
            Node node = nodes[i];
            OptimizedNode optimizedNode = new OptimizedNode(node);

            optimizedNode.breadth = GetSubNodesCount(nodes, i);
            optimizedNode.parent = nodes.ToList().IndexOf(node.parent);
            optimizedNode.index = i;
            optimizedNode.children = node.children.Select(node => node.priority - 1).ToArray();
            optimizedNode.relativeIndex = node.parent != null ? node.parent.children.IndexOf(node) : -1;

            if (typeof(Root).IsAssignableFrom(node.GetType()))
            {
                optimizedNode.typeCode = OptimizedNode.TypeCode.Root;
            }
            else if (typeof(Flow).IsAssignableFrom(node.GetType()))
            {
                optimizedNode.typeCode = OptimizedNode.TypeCode.Flow;
            }
            else if (typeof(Schema.Runtime.Action).IsAssignableFrom(node.GetType()))
            {
                optimizedNode.typeCode = OptimizedNode.TypeCode.Action;
            }

            cache[i] = optimizedNode;
        }

        return cache;
    }
    private static int GetSubNodesCount(Node[] flattenedNodes, int index)
    {
        if (index > flattenedNodes.Length - 1) return 0;

        bool found = false;
        Node node = flattenedNodes[index];
        int count = 0;

        while (!found)
        {
            index++;
            if (index > flattenedNodes.Length - 1) return count;

            if (!node.children.Contains(flattenedNodes[index]))
            {
                return count;
            }
            else
            {
                count++;
            }
        }

        return count;
    }
}

public class OptimizedGraph
{
    public OptimizedNode[] nodes;
}
public class OptimizedNode
{
    public int parent;
    public int[] children;
    public int index;
    public int relativeIndex;
    public int breadth;
    public OptimizedDecorator[] decorators;
    public TypeCode typeCode;
    public Node node;
    public OptimizedNode(Node node)
    {
        this.node = node;

        decorators = new OptimizedDecorator[node.decorators.Count];
        for (int i = 0; i < decorators.Length; i++)
        {
            decorators[i] = new OptimizedDecorator();
            decorators[i].node = this;
            decorators[i].decorator = node.decorators[i];
            decorators[i].nodePriority = i;
        }
    }

    public enum TypeCode
    {
        Root,
        Flow,
        Action
    }
}
public class OptimizedDecorator
{
    public OptimizedNode node;
    public int nodePriority;
    public Decorator decorator;
}