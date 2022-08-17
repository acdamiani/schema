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

                if (status == NodeStatus.Running || (_node != null && value.index - node.index < 0))
                    lastStatus[_node.index] = status;

                last = _node;
                _node = value;
            }
        }

        public ExecutableNode last { get; private set; }

        public NodeStatus status { get; set; }

        public NodeStatus? GetLastStatus(int index)
        {
            if (!lastStatus.ContainsKey(index))
                return null;

            return lastStatus[index];
        }
    }
}