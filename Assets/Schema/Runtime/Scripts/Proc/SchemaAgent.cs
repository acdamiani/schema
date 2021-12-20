using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using Schema;
using Schema.Runtime;

public class SchemaAgent : MonoBehaviour
{
    public Graph target;
    private OptimizedGraph graph;
    private int currentIndex;
    private bool firstCall = true;
    private List<Node> calledNodes = new List<Node>();
    private BlackboardData blackboardData;
#if UNITY_EDITOR
    [NonSerialized] public Node editorTarget;
#endif

    private Dictionary<string, object> agentState = new Dictionary<string, object>();
    private Dictionary<OptimizedDecorator, bool> decoratorState = new Dictionary<OptimizedDecorator, bool>();
    // Start is called before the first frame update
    // TODO: Implement
    public bool restartOnComplete;
    public int ticksPerSecond = 60;
    public int checksPerSecond = 60;
    public bool logTaskChanges;
    public int maxIterationsPerTick = 1000;
    public bool ignoreTickOverstep;
    private void Start()
    {
        if (!target) return;
        graph = SchemaManager.LoadGraph(target, false);

        blackboardData = new BlackboardData();
        blackboardData.Initialize(target.blackboard);

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
                    ((Schema.Runtime.Action)oNode.node).OnInitialize(agentState[oNode.node.uID], this);
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

        InvokeRepeating(nameof(EvaluateDecorators), 0f, 60f / checksPerSecond);
        InvokeRepeating(nameof(Tick), 0f, 60f / ticksPerSecond);
    }
    public List<Node> GetCalledNodes()
    {
        return calledNodes;
    }
    public BlackboardData GetBlackboardData()
    {
        return blackboardData;
    }

    //Ticks our graph
    private void Update()
    {
        if (!target) return;
        EvaluateDecorators();
        Tick();
    }
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

        bool ticked = false;
        int count = 0;

        int callerIndex = -1;

        NodeStatus context = NodeStatus.None;

        while (!ticked)
        {
            OptimizedNode node = graph.nodes[currentIndex];
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
                    Schema.Runtime.Action action = (Schema.Runtime.Action)node.node;

                    nodeCanRun = true;

                    if (firstCall)
                    {
                        nodeCanRun = NodeIsAllowedToRun(node);

                        firstCall = false;

                        if (nodeCanRun)
                            EnterNode(node, OptimizedNode.TypeCode.Action);

                        if (logTaskChanges)
                            Debug.Log($"Task {action.Name} reached on Agent {name} ");
                    }

                    if (!nodeCanRun)
                    {
                        firstCall = true;

                        context = NodeStatus.Failure;
                        currentIndex = node.parent;
                        callerIndex = node.relativeIndex;

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
        List<OptimizedDecorator> keys = new List<OptimizedDecorator>(decoratorState.Keys);

        foreach (OptimizedDecorator d in keys)
        {
            bool result = d.decorator.Evaluate(agentState[d.decorator.uID], this);
            if (result == decoratorState[d]) continue;
            decoratorState[d] = result;

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
                Schema.Runtime.Action action = (Schema.Runtime.Action)node.node;

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
                Schema.Runtime.Action action = (Schema.Runtime.Action)node.node;

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
}

namespace Schema.Runtime
{
    public enum NodeStatus
    {
        Success,
        Failure,
        Running,
        None
    }
}