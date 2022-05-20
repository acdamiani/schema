using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using Schema;
using Schema.Utilities;

public class SchemaAgent : MonoBehaviour
{
    public Graph target;
    private OptimizedGraph graph;
    private int currentIndex;
    private bool firstCall = true;
    private List<Node> calledNodes = new List<Node>();
#if UNITY_EDITOR
    [NonSerialized]
    public Node editorTarget;
    private Dictionary<string, bool?> nodeStatus = new Dictionary<string, bool?>();
#endif
    private Dictionary<string, object> agentState = new Dictionary<string, object>();
    private Dictionary<OptimizedDecorator, bool> decoratorState = new Dictionary<OptimizedDecorator, bool>();
    // TODO: Implement
    public bool restartOnComplete;
    public bool logTaskChanges;
    public int maxIterationsPerTick = 1000;
    public bool ignoreTickOverstep;
    private bool ticked;
    private int count;
    private int callerIndex;
    private static int pidInc;
    private int pid;
    NodeStatus context;
    private void Start()
    {
        pid = pidInc;
        pidInc++;

        if (!target) return;
        graph = SchemaManager.LoadGraph(target, false);

        BlackboardDataContainer.Initialize(target.blackboard);
        Blackboard global = Resources.Load<Blackboard>("GlobalBlackboard");
        if (global)
            BlackboardDataContainer.Initialize(global);

        foreach (OptimizedNode oNode in graph.nodes)
        {
            foreach (OptimizedDecorator oDecorator in oNode.decorators)
            {
                Type type = oDecorator.decorator.GetMemoryType();

                agentState.Add(oDecorator.decorator.uID, type == null ? null : Activator.CreateInstance(type));
            }

            Type t = oNode.node.GetMemoryType();

            //Check for dependencies on the nodes
            RequireAgentComponentAttribute attribute = oNode.node.GetType().GetCustomAttribute<RequireAgentComponentAttribute>();
            if (attribute != null)
            {
                foreach (Type attributeType in attribute.types)
                {
                    if (!gameObject.GetComponent(attributeType))
                        gameObject.AddComponent(attributeType);
                }
            }

            agentState.Add(oNode.node.uID, t == null ? null : Activator.CreateInstance(t));

            switch (oNode.typeCode)
            {
                case OptimizedNode.TypeCode.Action:
                    ((Schema.Action)oNode.node).OnInitialize(agentState[oNode.node.uID], this);
                    break;
                case OptimizedNode.TypeCode.Flow:
                    ((Flow)oNode.node).OnInitialize(agentState[oNode.node.uID], this);
                    break;
            }

            foreach (Decorator d in oNode.node.decorators)
            {
                d.OnInitialize(agentState[d.uID], this);
            }
        }

        VerifyComponents();

        // InvokeRepeating(nameof(EvaluateDecorators), 0f, 1f / checksPerSecond);
        // InvokeRepeating(nameof(Tick), 0f, 1f / ticksPerSecond);
    }
    void Update()
    {
        SchemaManager.pid = pid;

        if (target == null)
            return;

        EvaluateDecorators();
        Tick();
    }
    public List<Node> GetCalledNodes()
    {
        return calledNodes;
    }
#if UNITY_EDITOR
    public Dictionary<string, bool?> GetNodeStatus()
    {
        return nodeStatus;
    }
#endif
    void Reset()
    {
        VerifyComponents();
    }

    public void VerifyComponents()
    {
        if (!target) return;

        //Gather all classes that inherit from Node or Decorator
        IEnumerable<Type> nodeTypes = HelperMethods.GetEnumerableOfType(typeof(Node));
        IEnumerable<Type> decoratorTypes = HelperMethods.GetEnumerableOfType(typeof(Decorator));
        IEnumerable<Type> targetTypes = target.nodes?.Select(node => node.GetType()).Distinct();

        Type[] types = nodeTypes.Concat(decoratorTypes).ToArray();

        foreach (Type type in types)
        {
            if (targetTypes != null && !targetTypes.Contains(type)) continue;

            //Get RequireAgentComponent Attribute if it exists
            RequireAgentComponentAttribute a = type.GetCustomAttribute<RequireAgentComponentAttribute>();

            if (a == null) continue;

            //Check to see if it exists on the current gameObject
            foreach (Type attType in a.types)
            {
                Debug.Log($"{type}: {attType}");

                // if (gameObject.GetComponent(attType) == null)
                //     gameObject.AddComponent(attType);
            }
        }
    }
    private void Tick()
    {
        calledNodes.Clear();

        ticked = false;
        count = 0;
        callerIndex = -1;
        context = NodeStatus.None;

        while (!ticked)
        {
            OptimizedNode node = graph.nodes[currentIndex];
            SchemaManager.currentNode = node;
            SchemaManager.currentParentNode = node.parent >= 0 ? graph.nodes[node.parent] : null;
            string nodeID = node.node.uID;

            switch (node.typeCode)
            {
                case OptimizedNode.TypeCode.Root:
                    currentIndex++;
                    callerIndex = -1;
                    context = NodeStatus.None;
                    decoratorState.Clear();
                    break;
                case OptimizedNode.TypeCode.Flow:
                    Flow flow = (Flow)node.node;

                    bool nodeCanRun = true;

                    if (callerIndex == -1)
                    {
                        nodeCanRun = NodeIsAllowedToRun(node);

                        if (nodeCanRun)
                            EnterNode(node, OptimizedNode.TypeCode.Flow);
                    }

                    if (!nodeCanRun)
                    {
                        firstCall = true;

                        context = NodeStatus.Failure;
                        currentIndex = node.parent;
                        callerIndex = node.relativeIndex;

                        nodeStatus[node.node.uID] = false;

                        break;
                    }

                    int childIndex = flow.Tick(context, callerIndex);

                    if (childIndex == -1)
                    {
                        bool willRepeatNode = false;

                        for (int i = 0; i < node.decorators.Length; i++)
                        {
                            Decorator d = node.node.decorators[i];

                            willRepeatNode = node.node.decorators[i].OnNodeProcessed(agentState[d.uID], this, ref context) || willRepeatNode;
                        }

                        if (willRepeatNode)
                        {
                            currentIndex = node.index;
                            callerIndex = -1;
                        }
                        else
                        {
                            currentIndex = node.parent;
                            callerIndex = node.relativeIndex;
                        }

#if UNITY_EDITOR
                        nodeStatus[node.node.uID] = context == NodeStatus.Success;
#endif

                        ExitNode(node, OptimizedNode.TypeCode.Flow);
                    }
                    else
                    {
                        context = NodeStatus.None;
                        callerIndex = -1;
                        currentIndex = node.children[childIndex];
                        firstCall = true;
                    }
                    break;
                case OptimizedNode.TypeCode.Action:
                    Schema.Action action = (Schema.Action)node.node;

                    nodeCanRun = true;

                    if (firstCall)
                    {
                        nodeCanRun = NodeIsAllowedToRun(node);

                        firstCall = false;

                        if (nodeCanRun)
                            EnterNode(node, OptimizedNode.TypeCode.Action);

                        if (logTaskChanges)
                            Debug.Log($"Task {action.name} reached on Agent {name} ");
                    }

                    if (!nodeCanRun)
                    {
                        firstCall = true;

                        context = NodeStatus.Failure;
                        currentIndex = node.parent;
                        callerIndex = node.relativeIndex;

                        nodeStatus[node.node.uID] = false;

                        break;
                    }

                    context = action.Tick(agentState[nodeID], this);

                    calledNodes.Add(node.node);

                    if (context == NodeStatus.None)
                        Debug.LogWarning("A NodeStatus of None is treated the same as NodeStatus of Success. NodeStatus of None should not be included in Action nodes");

                    context = context == NodeStatus.None ? NodeStatus.Success : context;

                    if (context != NodeStatus.Running)
                    {
                        bool willRepeatNode = false;

                        for (int i = 0; i < node.decorators.Length; i++)
                        {
                            Decorator d = node.node.decorators[i];

                            willRepeatNode = node.node.decorators[i].OnNodeProcessed(agentState[d.uID], this, ref context) || willRepeatNode;
                        }

                        if (willRepeatNode)
                        {
                            currentIndex = node.index;
                            callerIndex = -1;
                        }
                        else
                        {
                            currentIndex = node.parent;
                            callerIndex = node.relativeIndex;
                        }

#if UNITY_EDITOR
                        nodeStatus[node.node.uID] = context == NodeStatus.Success;
#endif

                        firstCall = true;
                        ExitNode(node, OptimizedNode.TypeCode.Action);
                    }
                    else
                    {
                        ticked = true;
                    }

                    break;
            }

            if (count >= maxIterationsPerTick)
            {
                if (!ignoreTickOverstep)
                {
                    throw new UnityException(
                        $"The number of node steps taken in tree {target.name} on agent {name} have surpassed {maxIterationsPerTick}. You can increase this limit, or ignore it entirely in Agent settings."
                    );
                }
            }
            count++;
        }
    }

    private void EvaluateDecorators()
    {
        List<OptimizedDecorator> state = new List<OptimizedDecorator>(decoratorState.Keys);

        foreach (OptimizedDecorator d in state)
        {
            bool result = d.decorator.Evaluate(agentState[d.decorator.uID], this);
            if (result == decoratorState[d]) continue;
            decoratorState[d] = result;
            d.decorator.conditionalValue.value = result;

            if (currentIndex > d.node.index + d.node.breadth)
            {
                //Node is running to the right of the subtree
                if (d.decorator.abortsType == Decorator.ObserverAborts.Both || d.decorator.abortsType == Decorator.ObserverAborts.LowerPriority)
                {
                    PullToNode(d.node);
                }
            }
            else if (currentIndex <= d.node.index + d.node.breadth)
            {
                //Node is running under this subtree (or is the evaluated decorator)
                if (d.decorator.abortsType == Decorator.ObserverAborts.Both || d.decorator.abortsType == Decorator.ObserverAborts.Self)
                {
                    PullToNode(d.node);
                }
            }
        }
    }

    /// <summary>
    /// Determines whether to run the node based on the results of its decorators
    /// </summary>
    /// <param name="node">The node to check</param>
    /// <returns>Whether to run the node</returns>
    bool NodeIsAllowedToRun(OptimizedNode node)
    {
        bool result = true;

        for (int i = 0; i < node.decorators.Length; i++)
        {
            Decorator d = node.decorators[i].decorator;
            result = d.Evaluate(agentState[d.uID], this);

            decoratorState[node.decorators[i]] = result;
            d.conditionalValue.value = result;

            if (!result)
                break;
        }

        return result;
    }

    /// <summary>
    /// Start node execution
    /// </summary>
    /// <param name="node">The node to execute</param>
    /// <param name="typeCode">The type of the node</param>
    void EnterNode(OptimizedNode node, OptimizedNode.TypeCode typeCode)
    {
        //Enter the decorators, top to bottom
        for (int i = 0; i < node.decorators.Length; i++)
        {
            Decorator d = node.decorators[i].decorator;

            d.OnFlowEnter(agentState[d.uID], this);
        }

        switch (typeCode)
        {
            case OptimizedNode.TypeCode.Root:
                foreach (OptimizedDecorator d in graph.nodes[node.children[0]].decorators)
                    decoratorState[d] = false;
                break;
            case OptimizedNode.TypeCode.Flow:
                Flow flow = (Flow)node.node;

                for (int n = 0; n < node.children.Length; n++)
                {
                    OptimizedNode cur;
                    cur = graph.nodes[node.children[n]];

                    for (int j = 0; j < cur.decorators.Length; j++)
                    {
                        OptimizedDecorator d = cur.decorators[j];

                        decoratorState[d] = false;
                    }
                }

                flow.OnNodeEnter(agentState[flow.uID], this);
                break;
            case OptimizedNode.TypeCode.Action:
                Schema.Action action = (Schema.Action)node.node;

                action.OnNodeEnter(agentState[action.uID], this);
                break;
        }
    }
    /// <summary>
    /// End node execution
    /// </summary>
    /// <param name="node">The node to terminate execution for</param>
    /// <param name="typeCode">The type of the node</param>
    void ExitNode(OptimizedNode node, OptimizedNode.TypeCode typeCode)
    {
        //Enter the decorators, top to bottom
        for (int i = 0; i < node.decorators.Length; i++)
        {
            Decorator d = node.decorators[i].decorator;

            d.OnFlowExit(agentState[d.uID], this);
        }

        switch (typeCode)
        {
            case OptimizedNode.TypeCode.Root:
                foreach (OptimizedDecorator d in graph.nodes[node.children[0]].decorators)
                {
                    if (decoratorState.ContainsKey(d))
                        decoratorState.Remove(d);
                }
                break;
            case OptimizedNode.TypeCode.Flow:
                Flow flow = (Flow)node.node;

                for (int n = 0; n < node.children.Length; n++)
                {
                    OptimizedNode cur;
                    cur = graph.nodes[node.children[n]];

                    for (int j = 0; j < cur.decorators.Length; j++)
                    {
                        OptimizedDecorator d = cur.decorators[j];

                        if (decoratorState.ContainsKey(d))
                            decoratorState.Remove(d);
                    }
                }

                flow.OnNodeExit(agentState[flow.uID], this);
                break;
            case OptimizedNode.TypeCode.Action:
                Schema.Action action = (Schema.Action)node.node;

                action.OnNodeExit(agentState[action.uID], this);
                break;
        }
    }
    private void PullToNode(OptimizedNode node)
    {
        ExitNode(graph.nodes[currentIndex], graph.nodes[currentIndex].typeCode);

        currentIndex = node.index;
        firstCall = true;
    }
    private void OnDrawGizmos()
    {
        if (!editorTarget) return;

        editorTarget.DrawGizmos(this);

        foreach (Decorator d in editorTarget.decorators)
        {
            d.DrawGizmos(this);
        }
    }
    /// <summary>
    /// Get currently running node
    /// </summary>
    /// <returns>Currently running node</returns>
    public Node GetRunningNode()
    {
        if (currentIndex < 0 || currentIndex > graph.nodes.Length - 1)
            return null;

        return graph.nodes[currentIndex].node;
    }
}

namespace Schema
{
    public enum NodeStatus
    {
        Success,
        Failure,
        Running,
        None
    }
}
