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

        private readonly Dictionary<int, object[]> modifierMemory = new Dictionary<int, object[]>();

        public readonly Node node;

        private readonly Dictionary<int, object> nodeMemory = new Dictionary<int, object>();

        public ExecutableNode(Node node)
        {
            if (!node)
                throw new ArgumentNullException("node", "Node cannot be null!");

            this.node = node;

            Index = node.priority - 1;
            Children = node.children
                .Select(x => x.priority - 1)
                .OrderBy(x => x)
                .ToArray();
            Breadth = GetBreadth(node);
            NodeType = GetNodeType(node);

            dynamicConditionals = node.conditionals
                .Where(x => x.abortsType != Conditional.AbortsType.None)
                .Select(x => Array.IndexOf(node.conditionals, x))
                .ToArray();

            DynamicConditionalCount = dynamicConditionals.Length;

            if (node.parent)
            {
                RelativeIndex = Array.IndexOf(node.parent.children, node);
                Parent = node.parent.priority - 1;
            }
            else
            {
                RelativeIndex = -1;
                Parent = -1;
            }
        }

        public int Index { get; }
        public int RelativeIndex { get; }
        public int DynamicConditionalCount { get; }
        public int Parent { get; }
        public int[] Children { get; }
        public int Breadth { get; }
        public ExecutableNodeType NodeType { get; }

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

            switch (NodeType)
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

            foreach (int j in dynamicConditionals)
            {
                Conditional c = node.conditionals[j];

                bool isSub = current.IsSubTreeOf(node);
                bool isPriority = current.IsLowerPriority(node);

                switch (c.abortsType)
                {
                    case Conditional.AbortsType.Self when !isSub:
                    case Conditional.AbortsType.LowerPriority when !isPriority:
                    case Conditional.AbortsType.Both when !isPriority && !isSub:
                        continue;
                }

                bool status = c.Evaluate(conditionalMemory[id][j], context.agent);
                status = c.invert ? !status : status;

                if (!lastConditionalStatus.TryGetValue(j, out bool last)) last = status;
                lastConditionalStatus[j] = status;

                if (last == status) continue;

                switch (c.abortsWhen)
                {
                    case Conditional.AbortsWhen.OnSuccess when status:
                    case Conditional.AbortsWhen.OnFailure when !status:
                    case Conditional.AbortsWhen.Both:
                        return true;
                }
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
                return Index + 1;
            }

            int i;

            switch (NodeType)
            {
                case ExecutableNodeType.Root:
                    i = DoRoot(context);
                    break;
                case ExecutableNodeType.Flow:
                    i = DoFlow(id, context);
                    break;
                case ExecutableNodeType.Action:
                    i = DoAction(id, context, context.forceActionConditionalEvaluation);
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
            return Index + 1;
        }

        private int DoFlow(int id, ExecutionContext context)
        {
            Flow flow = node as Flow;

            if (flow == null)
                throw new Exception("Node is not of type Flow but the execution method was run anyways.");

            bool run = DoConditionalStack(id, context);

            if (!run)
            {
                context.status = NodeStatus.Failure;
                return Parent;
            }

            int diff = context.last.Index - Index;

            if (diff >= Breadth || diff < 0) flow.OnFlowEnter(nodeMemory[id], context.agent);

            int caller = -1;

            if (Children.Contains(context.last.Index))
                caller = context.last.RelativeIndex;

            int i = flow.Tick(nodeMemory[id], context.status, caller);

            int? child = i >= 0 && i < Children.Length ? Children[i] : null;

            Modifier.Message message = Modifier.Message.None;

            for (int j = 0; j < flow.modifiers.Length; j++)
                message = flow.modifiers[j].Modify(modifierMemory[id][j], context.agent, context.status);

            if (message == Modifier.Message.ForceFailure)
                context.status = NodeStatus.Failure;
            else if (message == Modifier.Message.ForceSuccess)
                context.status = NodeStatus.Success;

            bool repeat = message == Modifier.Message.Repeat;

            if (repeat)
                return Index;

            if (child == null)
            {
                flow.OnFlowExit(nodeMemory[id], context.agent);
                return Parent;
            }

            return child.Value;
        }

        private int DoAction(int id, ExecutionContext context, bool forceConditionalEvaluation)
        {
            Action action = node as Action;

            if (action == null)
                throw new Exception("Node is not of type Action but the execution method was run anyways.");

            bool run = true;

            if (context.last.Index != Index || forceConditionalEvaluation)
                run = DoConditionalStack(id, context);

            if (!run)
            {
                context.status = NodeStatus.Failure;
                return Parent;
            }

            if (context.last.Index != Index)
                action.OnNodeEnter(nodeMemory[id], context.agent);

            context.status = action.Tick(nodeMemory[id], context.agent);

            switch (context.status)
            {
                case NodeStatus.Running:
                    return Index;
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
                        return Index;

                    action.OnNodeExit(nodeMemory[id], context.agent);
                    return Parent;
            }
        }

        private bool DoConditionalStack(int id, ExecutionContext context)
        {
            bool run = true;

            for (int j = 0; j < node.conditionals.Length; j++)
            {
                run = node.conditionals[j].Evaluate(conditionalMemory[id][j], context.agent);
                run = node.conditionals[j].invert ? !run : run;

                lastConditionalStatus[j] = run;

                if (!run)
                    break;
            }

            return run;
        }

        private static ExecutableNodeType GetNodeType(Node node)
        {
            return node switch
            {
                Root => ExecutableNodeType.Root,
                Flow => ExecutableNodeType.Flow,
                Action => ExecutableNodeType.Action,
                _ => ExecutableNodeType.Invalid
            };
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