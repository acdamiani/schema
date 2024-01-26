using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Schema.Internal
{
    public class ExecutableTree
    {
        private readonly Dictionary<int, ExecutionContext> context;

        public ExecutableTree(Graph graph)
        {
            if (graph == null)
                throw new ArgumentNullException("graph", "Graph cannot be null!");

            tree = graph;
            nodes = graph.nodes
                .Select(x => new ExecutableNode(x))
                .Where(x => x.Index > -1)
                .OrderBy(x => x.Index)
                .ToArray();
            root = nodes
                .FirstOrDefault(x => x.NodeType == ExecutableNode.ExecutableNodeType.Root);
            blackboard = new ExecutableBlackboard(graph.blackboard);
            context = new Dictionary<int, ExecutionContext>();
        }

        public static ExecutableTree current { get; private set; }

        public Graph tree { get; }
        public ExecutableNode root { get; }
        public ExecutableNode[] nodes { get; }
        public ExecutableBlackboard blackboard { get; }

        public ExecutionContext GetExecutionContext(SchemaAgent agent)
        {
            int id = agent.GetInstanceID();

            context.TryGetValue(id, out ExecutionContext execution);

            if (execution == null)
                execution = context[id] = new ExecutionContext(agent);

            return execution;
        }

        public ExecutableNode GetExecutableNode(Node node)
        {
            return nodes
                .FirstOrDefault(x => x.node == node);
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

            context.node = nodes.FirstOrDefault(x => x.NodeType == ExecutableNode.ExecutableNodeType.Root);
        }

        public void Tick(SchemaAgent agent)
        {
            current = this;

            ExecutionContext context = GetExecutionContext(agent);
            ExecutionContext.current = context;

            if (context.node == null || agent.paused || agent.stopped)
                return;

            int t = 0;

            do
            {
                ExecutableNode abortTarget = null;

                for (int i = 0; i < nodes.Length; i++)
                {
                    ExecutableNode node = nodes[i];
                    if (node.DynamicConditionalCount <= 0) continue;

                    if (node.RunDynamicConditionals(context)) abortTarget = node;
                    else i += node.Breadth - 1;
                }

                if (abortTarget != null)
                {
                    context.RemoveStatus(context.node);
                    context.node = abortTarget;
                    context.forceActionConditionalEvaluation = true;
                }

                int next = nodes[context.node.Index].Execute(context);
                next = next > nodes.Length - 1 ? 0 : next;

                context.forceActionConditionalEvaluation = false;

                context.node = nodes[next];

                if (++t == agent.maxStepsPerTick)
                {
                    Debug.LogWarningFormat(
                        "The behavior tree {0} has been stepped through over {1} times in one frame! Make sure that there are Action nodes in the tree that take time to execute.",
                        tree.name,
                        t
                    );

                    return;
                }
            } while (context.last != context.node && !(agent.paused || agent.stopped));
        }
    }
}