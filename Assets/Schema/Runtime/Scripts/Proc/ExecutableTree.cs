using System.Linq;
using System.Collections.Generic;

namespace Schema.Internal
{
    public class ExecutableTree
    {
        public static ExecutableTree current { get { return _current; } }
        private static ExecutableTree _current;
        public ExecutableNode[] nodes { get; }
        public ExecutableBlackboard blackboard { get; }
        private Dictionary<int, ExecutionContext> context { get; }
        public ExecutableTree(Graph graph)
        {
            nodes = graph.nodes
                .Select(x => new ExecutableNode(x))
                .OrderBy(x => x.index)
                .ToArray();
            blackboard = new ExecutableBlackboard(graph.blackboard);
            context = new Dictionary<int, ExecutionContext>();
        }
        public ExecutionContext GetExecutionContext(SchemaAgent agent)
        {
            int id = agent.GetInstanceID();

            context.TryGetValue(id, out ExecutionContext execution);

            if (execution == null)
                execution = context[id] = new ExecutionContext(agent);

            return execution;
        }
        public void Initialize(SchemaAgent agent)
        {
            ExecutionContext context = GetExecutionContext(agent);

            ExecutableNode node;

            for (int i = 0; i < nodes.Length; i++)
            {
                node = nodes[i];
                node.Initialize(context);
            }

            context.node = nodes.FirstOrDefault(x => x.nodeType == ExecutableNode.ExecutableNodeType.Root);
        }
        public void Tick(SchemaAgent agent)
        {
            _current = this;

            ExecutionContext context = GetExecutionContext(agent);
            ExecutionContext.current = context;

            if (context.node == null)
                return;

            do
            {
                int i = nodes[context.node.index].Execute(context);
                i = i > nodes.Length - 1 ? 0 : i;

                context.last = context.node;
                context.node = nodes[i];
            } while (context.last != context.node);
        }
    }
}