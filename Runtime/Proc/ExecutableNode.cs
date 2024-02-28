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

        private readonly Dictionary<int, object[]> _conditionalMemory = new Dictionary<int, object[]>();
        private readonly int[] _dynamicConditionals;
        private readonly Dictionary<int, bool> _lastConditionalStatus = new Dictionary<int, bool>();
        private readonly Dictionary<int, object[]> _modifierMemory = new Dictionary<int, object[]>();

        public readonly Node Node;

        public ExecutableNode(Node node)
        {
            if (!node)
                throw new ArgumentNullException("node", "Node cannot be null!");

            Node = node;

            Index = node.priority - 1;
            Children = node.children
                .Select(x => x.priority - 1)
                .OrderBy(x => x)
                .ToArray();
            Breadth = GetBreadth(node);
            NodeType = GetNodeType(node);
            Memory = new Dictionary<int, object>();

            _dynamicConditionals = node.conditionals
                .Where(x => x.abortsType != Conditional.AbortsType.None)
                .Select(x => Array.IndexOf(node.conditionals, x))
                .ToArray();

            if (node.parent != null)
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
        public int Parent { get; }
        public int[] Children { get; }
        public int Breadth { get; }
        public ExecutableNodeType NodeType { get; }
        public Dictionary<int, object> Memory { get; }

        public void Initialize(ExecutionContext context)
        {
            int id = context.Agent.GetInstanceID();

            if (!Memory.ContainsKey(id))
            {
                Type memType = Node.GetType().GetMemoryType();
                Memory[id] = memType == null ? null : Activator.CreateInstance(memType);
            }

            if (!_conditionalMemory.ContainsKey(id))
            {
                _conditionalMemory[id] = new object[Node.conditionals.Length];

                for (int i = 0; i < Node.conditionals.Length; i++)
                {
                    Type memType = Node.conditionals[i].GetType().GetMemoryType();
                    _conditionalMemory[id][i] = memType == null ? null : Activator.CreateInstance(memType);
                }
            }

            if (!_modifierMemory.ContainsKey(id))
            {
                _modifierMemory[id] = new object[Node.modifiers.Length];

                for (int i = 0; i < Node.modifiers.Length; i++)
                {
                    Type memType = Node.modifiers[i].GetType().GetMemoryType();
                    _modifierMemory[id][i] = memType == null ? null : Activator.CreateInstance(memType);
                }
            }

            switch (NodeType)
            {
                case ExecutableNodeType.Root:
                    break;
                case ExecutableNodeType.Flow:
                    InitializeFlow(id, context.Agent);
                    break;
                case ExecutableNodeType.Action:
                    InitializeAction(id, context.Agent);
                    break;
                case ExecutableNodeType.Invalid:
                default:
                    throw new Exception("Node is not of a valid type!");
            }
        }

        public bool? GetLastConditionalStatus(int index)
        {
            if (!_lastConditionalStatus.ContainsKey(index))
                return null;

            return _lastConditionalStatus[index];
        }

        private void InitializeFlow(int id, SchemaAgent agent)
        {
            Flow flow = Node as Flow;

            if (flow == null)
                throw new Exception("Node is not of type Flow but the initialization method was run anyways.");

            flow.OnInitialize(Memory[id], agent);

            for (int i = 0; i < flow.modifiers.Length; i++)
                flow.modifiers[i].OnInitialize(_modifierMemory[id][i], agent);

            for (int i = 0; i < flow.conditionals.Length; i++)
                flow.conditionals[i].OnInitialize(_conditionalMemory[id][i], agent);
        }

        private void InitializeAction(int id, SchemaAgent agent)
        {
            Action action = Node as Action;

            if (action == null)
                throw new Exception("Node is not of type Action but the initialization method was run anyways.");

            action.OnInitialize(Memory[id], agent);

            for (int i = 0; i < action.modifiers.Length; i++)
                action.modifiers[i].OnInitialize(_modifierMemory[id][i], agent);

            for (int i = 0; i < action.conditionals.Length; i++)
                action.conditionals[i].OnInitialize(_conditionalMemory[id][i], agent);
        }

        public bool RunDynamicConditionals(ExecutionContext context)
        {
            Node current = context.Node.Node;
            int id = context.Agent.GetInstanceID();

            foreach (int j in _dynamicConditionals)
            {
                Conditional c = Node.conditionals[j];

                bool isSub = current.IsSubTreeOf(Node);
                bool isPriority = current.IsLowerPriority(Node);

                switch (c.abortsType)
                {
                    case Conditional.AbortsType.Self when !isSub:
                    case Conditional.AbortsType.LowerPriority when !isPriority:
                    case Conditional.AbortsType.Both when !isPriority && !isSub:
                        continue;
                }

                bool status = c.Evaluate(_conditionalMemory[id][j], context.Agent);
                status = c.invert ? !status : status;

                if (!_lastConditionalStatus.TryGetValue(j, out bool last)) last = status;
                _lastConditionalStatus[j] = status;

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
            int id = context.Agent.GetInstanceID();

            if (!Memory.ContainsKey(id))
            {
                Debug.LogFormat(
                    "Agent {0} does not have a memory instance. This means that the initialization method was not run for this agent.",
                    context.Agent.name);
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
                    i = DoAction(id, context, context.ForceActionConditionalEvaluation);
                    break;
                case ExecutableNodeType.Invalid:
                default:
                    throw new Exception("Node is not of a valid type!");
            }

            return i;
        }

        private int DoRoot(ExecutionContext context)
        {
            if (context.Last != null)
            {
                if (!context.Agent.restartWhenComplete)
                {
                    context.Agent.Stop();
                }
                else
                {
                    if (context.Agent.treePauseTime > 0f)
                        context.Agent.Pause(context.Agent.treePauseTime);

                    if (context.Agent.resetBlackboardOnRestart)
                        context.Agent.tree.blackboard.Reset();
                }
            }

            context.Status = NodeStatus.Success;
            return Index + 1;
        }

        private int DoFlow(int id, ExecutionContext context)
        {
            Flow flow = Node as Flow;

            if (!flow)
                throw new Exception("Node is not of type Flow but the execution method was run anyways.");

            bool run = DoConditionalStack(id, context);

            if (!run)
            {
                context.Status = NodeStatus.Failure;
                return Parent;
            }

            int caller = -1;

            if (Children.Contains(context.Last.Index))
                caller = context.Last.RelativeIndex;

            int i = flow.Tick(Memory[id], context.Status, caller);

            int? child = i >= 0 && i < Children.Length ? Children[i] : null;

            Modifier.Message message = Modifier.Message.None;

            for (int j = 0; j < flow.modifiers.Length; j++)
                message = flow.modifiers[j].Modify(_modifierMemory[id][j], context.Agent, context.Status);

            context.Status = message switch
            {
                Modifier.Message.ForceFailure => NodeStatus.Failure,
                Modifier.Message.ForceSuccess => NodeStatus.Success,
                _ => context.Status
            };

            bool repeat = message == Modifier.Message.Repeat;

            if (repeat)
                return Index;

            return child ?? Parent;
        }

        private int DoAction(int id, ExecutionContext context, bool forceConditionalEvaluation)
        {
            Action action = Node as Action;

            if (action == null)
                throw new Exception("Node is not of type Action but the execution method was run anyways.");

            bool run = true;

            if (context.Last.Index != Index || forceConditionalEvaluation)
                run = DoConditionalStack(id, context);

            if (!run)
            {
                context.Status = NodeStatus.Failure;
                return Parent;
            }

            context.Status = action.Tick(Memory[id], context.Agent);

            switch (context.Status)
            {
                case NodeStatus.Running:
                    return Index;
                case NodeStatus.Success:
                case NodeStatus.Failure:
                default:
                    Modifier.Message message = Modifier.Message.None;

                    for (int j = 0; j < action.modifiers.Length; j++)
                        message = action.modifiers[j].Modify(_modifierMemory[id][j], context.Agent, context.Status);

                    if (message == Modifier.Message.ForceFailure)
                        context.Status = NodeStatus.Failure;
                    else if (message == Modifier.Message.ForceSuccess)
                        context.Status = NodeStatus.Success;

                    bool repeat = message == Modifier.Message.Repeat;

                    if (repeat)
                        return Index;

                    return Parent;
            }
        }

        private bool DoConditionalStack(int id, ExecutionContext context)
        {
            bool run = true;

            for (int j = 0; j < Node.conditionals.Length; j++)
            {
                run = Node.conditionals[j].Evaluate(_conditionalMemory[id][j], context.Agent);
                run = Node.conditionals[j].invert ? !run : run;

                _lastConditionalStatus[j] = run;

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