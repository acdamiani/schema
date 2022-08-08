using UnityEngine;
using Schema.Utilities;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Schema.Internal
{
    public class ExecutableNode
    {
        public int index { get; }
        public int relativeIndex { get; }
        public int parent { get; }
        public int[] children { get; }
        public int breadth { get; }
        public ExecutableNodeType nodeType { get; }
        private Node node;
        private Dictionary<int, object> nodeMemory
            = new Dictionary<int, object>();
        private Dictionary<int, object[]> conditionalMemory
            = new Dictionary<int, object[]>();
        private Dictionary<int, object[]> modifierMemory
            = new Dictionary<int, object[]>();
        public ExecutableNode(Node node)
        {
            this.node = node;

            index = node.priority - 1;
            children = node.children
                .Select(x => x.priority - 1)
                .OrderBy(x => x)
                .ToArray();
            breadth = GetBreadth(node);
            nodeType = GetNodeType(node);

            if (node.parent != null)
            {
                relativeIndex = Array.IndexOf(node.parent.children, node);
                parent = node.parent.priority - 1;
            }
            else
            {
                relativeIndex = -1;
                parent = -1;
            }
        }
        public void Initialize(ExecutionContext context)
        {
            int id = context.agent.GetInstanceID();

            if (!nodeMemory.ContainsKey(id))
            {
                Type memType = node.GetType().GetMemoryType();
                nodeMemory[id] = memType == null ? null : Activator.CreateInstance(memType);
            }

            if (!conditionalMemory.ContainsKey(id))
            {
                conditionalMemory[id] = new object[node.conditionals.Length];

                for (int i = 0; i < node.conditionals.Length; i++)
                {
                    Type memType = node.conditionals[i].GetType().GetMemoryType();
                    conditionalMemory[id][i] = memType == null ? null : Activator.CreateInstance(memType);
                }
            }

            if (!modifierMemory.ContainsKey(id))
            {
                modifierMemory[id] = new object[node.modifiers.Length];

                for (int i = 0; i < node.modifiers.Length; i++)
                {
                    Type memType = node.modifiers[i].GetType().GetMemoryType();
                    modifierMemory[id][i] = memType == null ? null : Activator.CreateInstance(memType);
                }
            }

            switch (nodeType)
            {
                case ExecutableNodeType.Root:
                    break;
                case ExecutableNodeType.Flow:
                    InitializeFlow(id, context.agent);
                    break;
                case ExecutableNodeType.Action:
                    InitializeAction(id, context.agent);
                    break;
                case ExecutableNodeType.Invalid:
                default:
                    throw new Exception("Node is not of a valid type!");
            }
        }
        private void InitializeFlow(int id, SchemaAgent agent)
        {
            Flow flow = node as Flow;

            if (flow == null)
                throw new Exception("Node is not of type Flow but the initialization method was run anyways.");

            flow.OnInitialize(nodeMemory[id], agent);

            for (int i = 0; i < flow.modifiers.Length; i++)
                flow.modifiers[i].OnInitialize(modifierMemory[id][i], agent);

            for (int i = 0; i < flow.conditionals.Length; i++)
                flow.conditionals[i].OnInitialize(conditionalMemory[id][i], agent);
        }
        private void InitializeAction(int id, SchemaAgent agent)
        {
            Action action = node as Action;

            if (action == null)
                throw new Exception("Node is not of type Action but the initialization method was run anyways.");

            action.OnInitialize(nodeMemory[id], agent);

            for (int i = 0; i < action.modifiers.Length; i++)
                action.modifiers[i].OnInitialize(modifierMemory[id][i], agent);

            for (int i = 0; i < action.conditionals.Length; i++)
                action.conditionals[i].OnInitialize(conditionalMemory[id][i], agent);
        }
        public int Execute(ExecutionContext context)
        {
            int id = context.agent.GetInstanceID();

            if (!nodeMemory.ContainsKey(id))
            {
                Debug.LogFormat("Agent {0} does not have a memory instance. This means that the initialization method was not run for this agent.", context.agent.name);
                return index + 1;
            }

            int i;

            switch (nodeType)
            {
                case ExecutableNodeType.Root:
                    i = DoRoot(context);
                    break;
                case ExecutableNodeType.Flow:
                    i = DoFlow(id, context);
                    break;
                case ExecutableNodeType.Action:
                    i = DoAction(id, context);
                    break;
                case ExecutableNodeType.Invalid:
                default:
                    throw new Exception("Node is not of a valid type!");
            }

            return i;
        }
        private int DoRoot(ExecutionContext context)
        {
            context.status = NodeStatus.Success;
            return index + 1;
        }
        private int DoFlow(int id, ExecutionContext context)
        {
            Flow flow = node as Flow;

            if (flow == null)
                throw new Exception("Node is not of type Flow but the execution method was run anyways.");

            bool run = true;

            for (int j = 0; j < flow.conditionals.Length; j++)
            {
                run = flow.conditionals[j].Evaluate(conditionalMemory[id][j], context.agent);

                if (!run)
                    break;
            }

            if (!run)
            {
                context.status = NodeStatus.Failure;
                return parent;
            }

            int diff = context.last.index - index;

            if (diff >= breadth || diff < 0)
            {
                flow.OnFlowEnter(nodeMemory[id], context.agent);
            }

            int caller = -1;

            if (children.Contains(context.last.index))
                caller = context.last.relativeIndex;

            int i = flow.Tick(nodeMemory[id], context.status, caller);

            int? child = i >= 0 && i < children.Length ? children[i] : null;

            Modifier.Message message = Modifier.Message.None;

            for (int j = 0; j < flow.modifiers.Length; j++)
                message = flow.modifiers[j].Modify(modifierMemory[id][j], context.agent, context.status);

            if (message == Modifier.Message.ForceFailure)
                context.status = NodeStatus.Failure;
            else if (message == Modifier.Message.ForceSuccess)
                context.status = NodeStatus.Success;

            bool repeat = message == Modifier.Message.Repeat;

            if (repeat)
                return index;

            if (child == null)
            {
                flow.OnFlowExit(nodeMemory[id], context.agent);
                return parent;
            }

            return child.Value;
        }
        private int DoAction(int id, ExecutionContext context)
        {
            Action action = node as Action;

            if (action == null)
                throw new Exception("Node is not of type Action but the execution method was run anyways.");

            if (context.last.index != index)
                action.OnNodeEnter(nodeMemory[id], context.agent);

            context.status = action.Tick(nodeMemory[id], context.agent);

            switch (context.status)
            {
                case NodeStatus.Running:
                    return index;
                case NodeStatus.Success:
                case NodeStatus.Failure:
                default:
                    Modifier.Message message = Modifier.Message.None;

                    for (int j = 0; j < action.modifiers.Length; j++)
                        message = action.modifiers[j].Modify(modifierMemory[id][j], context.agent, context.status);

                    if (message == Modifier.Message.ForceFailure)
                        context.status = NodeStatus.Failure;
                    else if (message == Modifier.Message.ForceSuccess)
                        context.status = NodeStatus.Success;

                    if (action.GetType() == typeof(Schema.Builtin.Nodes.Wait))
                        Debug.Log(message.ToString());

                    bool repeat = message == Modifier.Message.Repeat;

                    if (repeat)
                        return index;

                    action.OnNodeExit(nodeMemory[id], context.agent);
                    return parent;
            }
        }
        private static ExecutableNodeType GetNodeType(Node node)
        {
            if (node is Root)
                return ExecutableNodeType.Root;
            else if (node is Flow)
                return ExecutableNodeType.Flow;
            else if (node is Action)
                return ExecutableNodeType.Action;

            return ExecutableNodeType.Invalid;
        }
        private static int GetBreadth(Node node)
        {
            Node parent = node.parent;

            if (parent == null)
                return node.graph.nodes.Length;

            int i = Array.IndexOf(parent.children, node);

            if (i + 1 > parent.children.Length - 1)
                return 1;

            return parent.children[i + 1].priority - node.priority;
        }
        public enum ExecutableNodeType
        {
            Root,
            Flow,
            Action,
            Invalid
        }
    }
}