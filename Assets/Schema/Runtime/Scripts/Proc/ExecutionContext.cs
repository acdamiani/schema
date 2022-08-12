using System.Collections.Generic;

namespace Schema.Internal
{
    public class ExecutionContext
    {
        private readonly Dictionary<int, NodeStatus> lastStatus;
        private ExecutableNode _node;
        private NodeStatus _status;

        public ExecutionContext(SchemaAgent agent)
        {
            this.agent = agent;

            _status = NodeStatus.Success;
            lastStatus = new Dictionary<int, NodeStatus>();
        }

        public static ExecutionContext current { get; set; }
        public SchemaAgent agent { get; }

        public ExecutableNode node
        {
            get => _node;
            set
            {
                _node = value;
                lastStatus[_node.index] = status;
            }
        }

        public ExecutableNode last { get; set; }

        public NodeStatus status
        {
            get => _status;
            set
            {
                _status = value;
                lastStatus[_node.index] = status;
            }
        }

        public NodeStatus? GetLastStatus(int index)
        {
            if (!lastStatus.ContainsKey(index))
                return null;

            return lastStatus[index];
        }
    }
}