using System;
using System.Collections.Generic;
using System.Linq;
using Schema.Utilities;
using UnityEngine;

namespace Schema.Internal
{
    public class ExecutableNode
    {
        public enum ExecutableNodeType
        {
            Root,
            Flow,
            Action,
            Invalid
        }

        private readonly Dictionary<int, object[]> conditionalMemory = new Dictionary<int, object[]>();

        private readonly int[] dynamicConditionals;

        private readonly Dictionary<int, bool> lastConditionalStatus = new Dictionary<int, bool>();

        private readonly Dictionary<int, bool> lastDynamicStatus = new Dictionary<int, bool>();

        private readonly Dictionary<int, object[]> modifierMemory = new Dictionary<int, object[]>();

        public readonly Node node;

        private readonly Dictionary<int, object> nodeMemory = new Dictionary<int, object>();

        public ExecutableNode(Node node)
        {
            if (node == null)
                throw new ArgumentNullException("node", "Node cannot be null!");

            this.node = node;

            index = node.priority - 1;
            children = node.children
                .Select(x => x.priority - 1)
                .OrderBy(x => x)
                .ToArray();
            breadth = GetBreadth(node);
            nodeType = GetNodeType(node);

            dynamicConditionals = node.conditionals
                .Where(x => x.abortsType != Conditional.AbortsType.None)
                .Select(x => Array.IndexOf(node.conditionals, x))
                .ToArray();

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

        public int index { get; }
        public int relativeIndex { get; }
        public int parent { get; }
        public int[] children { get; }
        public int breadth { get; }
        public ExecutableNodeType nodeType { get; }

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

        public bool? GetLastConditionalStatus(int index)
        {
            if (!lastConditionalStatus.ContainsKey(index))
                return null;

            return lastConditionalStatus[index];
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

        public bool RunDynamicConditionals(ExecutionContext context)
        {
            Node current = context.node.node;
            int id = context.agent.GetInstanceID();

            for (int i = 0; i < dynamicConditionals.Length; i++)
            {
                int j = dynamicConditionals[i];
                Conditional c = node.conditionals[j];

                bool isSubAbort = c.abortsType == Conditional.AbortsType.Self ||
                                  c.abortsType == Conditional.AbortsType.Both;
                bool isPriorityAbort = c.abortsType == Conditional.AbortsType.LowerPriority ||
                                       c.abortsType == Conditional.AbortsType.Both;

                bool isSub = current.IsSubTreeOf(node);
                bool isPriority = current.IsLowerPriority(node);

                if (
                    !(isSubAbort && isPriorityAbort && (isSub || isPriority))
                    && (
                        (isSubAbort && !isSub)
                        || (isPriorityAbort && !isPriority)
                    )
                )
                    continue;

                bool status = c.Evaluate(conditionalMemory[id][j], context.agent);
                status = c.invert ? !status : status;

                lastDynamicStatus.TryGetValue(j, out bool last);
                lastDynamicStatus[j] = status;

                bool abortOnSuccess = c.abortsWhen == Conditional.AbortsWhen.OnSuccess ||
                                      c.abortsWhen == Conditional.AbortsWhen.Both;
                bool abortOnFailure = c.abortsWhen == Conditional.AbortsWhen.OnFailure ||
                                      c.abortsWhen == Conditional.AbortsWhen.Both;

                if (status != last
                    && ((status && abortOnSuccess) || (!status && abortOnFailure))
                    && ((isSubAbort && isSub) || (isPriorityAbort && isPriority))
                   )
                    return true;
            }

            return false;
        }

        public int Execute(ExecutionContext context)
        {
            int id = context.agent.GetInstanceID();

            if (!nodeMemory.ContainsKey(id))
            {
                Debug.LogFormat(
                    "Agent {0} does not have a memory instance. This means that the initialization method was not run for this agent.",
                    context.agent.name);
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
            if (context.last != null)
            {
                if (!context.agent.restartWhenComplete)
                {
                    context.agent.Stop();
                }
                else
                {
                    if (context.agent.treePauseTime > 0f)
                        context.agent.Pause(context.agent.treePauseTime);

                    if (context.agent.resetBlackboardOnRestart)
                        context.agent.tree.blackboard.Reset();
                }
            }

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
                run = flow.conditionals[j].invert ? !run : run;

                lastConditionalStatus[j] = run;

                if (!run)
                    break;
            }

            if (!run)
            {
                context.status = NodeStatus.Failure;
                return parent;
            }

            int diff = context.last.index - index;

            if (diff >= breadth || diff < 0) flow.OnFlowEnter(nodeMemory[id], context.agent);

            int caller = -1;

            if (children.Contains(context.last.index))
                caller = context.last.relativeIndex;

            int i = flow.Tick(nodeMemory[id], context.status, caller);

            int? child = i >= 0 && i < children.Length ? children[i] : (int?)null;

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

            bool run = true;

            if (context.last.index != index)
                for (int j = 0; j < action.conditionals.Length; j++)
                {
                    run = action.conditionals[j].Evaluate(conditionalMemory[id][j], context.agent);
                    run = action.conditionals[j].invert ? !run : run;

                    lastConditionalStatus[j] = run;

                    if (!run)
                        break;
                }

            if (!run)
            {
                context.status = NodeStatus.Failure;
                return parent;
            }

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
            if (node is Flow)
                return ExecutableNodeType.Flow;
            if (node is Action)
                return ExecutableNodeType.Action;

            return ExecutableNodeType.Invalid;
        }

        private static int GetBreadth(Node node)
        {
            int count = 1;

            for (int i = 0; i < node.children.Length; i++)
                count += GetBreadth(node.children[i]);

            return count;
        }
    }
}