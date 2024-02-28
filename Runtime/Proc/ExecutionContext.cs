using System.Collections.Generic;

namespace Schema.Internal
{
    public class ExecutionContext
    {
        private readonly Dictionary<int, NodeStatus> _lastStatus;

        private ExecutableNode _node;

        public ExecutionContext(SchemaAgent agent)
        {
            Agent = agent;

            Status = NodeStatus.Success;
            _lastStatus = new Dictionary<int, NodeStatus>();
        }

        public static ExecutionContext Current { get; set; }
        public SchemaAgent Agent { get; }

        public ExecutableNode Node
        {
            get => _node;
            set
            {
                if (value == null || value == _node)
                    return;

                if (Status == NodeStatus.Running || (_node != null && value.Index - _node.Index < 0))
                {
                    _lastStatus[_node.Index] = Status;

                    if (value.Index == 0)
                        _lastStatus[0] = Status;
                }

                StepOut(_node);
                StepIn(value);

                Last = _node;
                _node = value;
            }
        }

        public ExecutableNode Last { get; private set; }

        public NodeStatus Status { get; set; }

        public bool ForceActionConditionalEvaluation { get; set; }

        public void ForceStatus(ExecutableNode node)
        {
            if (node.Index > 0)
                _lastStatus[node.Index] = Status;
        }

        public void RemoveStatus(ExecutableNode node)
        {
            if (node.Index > 0 && _lastStatus.ContainsKey(node.Index))
                _lastStatus.Remove(node.Index);
        }

        public NodeStatus? GetLastStatus(int index)
        {
            if (!_lastStatus.ContainsKey(index))
                return null;

            return _lastStatus[index];
        }

        private void StepIn(ExecutableNode node)
        {
            if (!node.Memory.TryGetValue(Agent.GetInstanceID(), out object memory))
                return;

            switch (node.Node)
            {
                case Flow flow:
                    flow.OnFlowEnter(memory, Agent);
                    break;
                case Action action:
                    action.OnNodeEnter(memory, Agent);
                    break;
                default:
                    return;
            }
        }

        private void StepOut(ExecutableNode node)
        {
            if (!node.Memory.TryGetValue(Agent.GetInstanceID(), out object memory))
                return;

            switch (node.Node)
            {
                case Flow flow:
                    flow.OnFlowExit(memory, Agent);
                    break;
                case Action action:
                    action.OnNodeExit(memory, Agent);
                    break;
                default:
                    return;
            }
        }
    }
}