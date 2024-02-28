using System.Collections.Generic;

namespace Schema.Internal
{
    public class ExecutionContext
    {
        private readonly Dictionary<int, NodeStatus> lastStatus;
        private ExecutableNode _node;

        public ExecutionContext(SchemaAgent agent)
        {
            this.agent = agent;

            status = NodeStatus.Success;
            lastStatus = new Dictionary<int, NodeStatus>();
        }

        public static ExecutionContext current { get; set; }
        public SchemaAgent agent { get; }

        public ExecutableNode node
        {
            get => _node;
            set
            {
                if (value == null)
                    return;

                if (status == NodeStatus.Running || (_node != null && value.Index - node.Index < 0))
                {
                    lastStatus[_node.Index] = status;

                    if (value.Index == 0)
                        lastStatus[0] = status;
                }

                last = _node;
                _node = value;
            }
        }

        public ExecutableNode last { get; private set; }

        public NodeStatus status { get; set; }

        public bool forceActionConditionalEvaluation { get; set; }

        public void ForceStatus(ExecutableNode node)
        {
            if (node.Index > 0)
                lastStatus[node.Index] = status;
        }

        public void RemoveStatus(ExecutableNode node)
        {
            if (node.Index > 0 && lastStatus.ContainsKey(node.Index))
                lastStatus.Remove(node.Index);
        }

        public NodeStatus? GetLastStatus(int index)
        {
            if (!lastStatus.ContainsKey(index))
                return null;

            return lastStatus[index];
        }
    }
}